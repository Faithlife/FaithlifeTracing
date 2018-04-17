#addin "Cake.Git"
#addin nuget:?package=Cake.XmlDocMarkdown&version=1.2.1

using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var trigger = Argument("trigger", "");

var solutionFileName = "Faithlife.Tracing.sln";
var docsProjects = new[] { "Faithlife.Tracing" };
var docsRepoUri = "https://github.com/Faithlife/FaithlifeTracing.git";
var docsSourceUri = "https://github.com/Faithlife/FaithlifeTracing/tree/master/src/Faithlife.Tracing";

var nugetSource = "https://api.nuget.org/v3/index.json";
var buildBotUserName = "faithlifebuildbot";
var buildBotPassword = EnvironmentVariable("BUILD_BOT_PASSWORD");

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("src/**/bin");
		CleanDirectories("src/**/obj");
		CleanDirectories("tests/**/bin");
		CleanDirectories("tests/**/obj");
		CleanDirectories("release");
	});

Task("Build")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName);
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings { Configuration = configuration, ArgumentCustomization = args => args.Append("--verbosity normal") });
	});

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("UpdateDocs")
	.WithCriteria(!string.IsNullOrEmpty(buildBotPassword))
	.WithCriteria(EnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var branchName = "gh-pages";
		var docsDirectory = new DirectoryPath(branchName);
		GitClone(docsRepoUri, docsDirectory, new GitCloneSettings { BranchName = branchName });

		Information($"Updating documentation at {docsDirectory}.");
		foreach (var docsProject in docsProjects)
		{
			XmlDocMarkdownGenerate(File($"src/{docsProject}/bin/{configuration}/net461/{docsProject}.dll").ToString(), $"{docsDirectory}{System.IO.Path.DirectorySeparatorChar}",
				new XmlDocMarkdownSettings { SourceCodePath = $"{docsSourceUri}/{docsProject}", NewLine = "\n", ShouldClean = true });
		}

		if (GitHasUncommitedChanges(docsDirectory))
		{
			Information("Committing all documentation changes.");
			GitAddAll(docsDirectory);
			GitCommit(docsDirectory, "Faithlife Build Bot", "faithlifebuildbot@users.noreply.github.com", "Automatic documentation update.");
			Information("Pushing updated documentation to GitHub.");
			GitPush(docsDirectory, buildBotUserName, buildBotPassword, branchName);
		}
		else
		{
			Information("No documentation changes detected.");
		}
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		foreach (var projectPath in GetFiles("tests/**/*.csproj").Select(x => x.FullPath))
			DotNetCoreTest(projectPath, new DotNetCoreTestSettings { Configuration = configuration });
	});

Task("NuGetPackage")
	.IsDependentOn("Rebuild")
	.IsDependentOn("Test")
	.IsDependentOn("UpdateDocs")
	.Does(() =>
	{
		// look for a tag of the form "(prefix-)v1.0.0(-suffix)", where 'prefix' is a project name such as "data" or "aspnetmvc", and 'suffix' is a SemVer suffix
		var projectSuffix = "";
		var versionSuffix = "";
		if (!string.IsNullOrEmpty(trigger))
		{
			var match = Regex.Match(trigger, @"^(?:([a-z.]+)-)?v[^\.]+\.[^\.]+\.[^\.]+-(.+)");
			if (match.Success)
			{
				projectSuffix = match.Groups[1].ToString();
				versionSuffix = match.Groups[2].ToString();
			}
		}

		// pack only the project identified by the current tag being built
		foreach (var projectFile in GetFiles("src/**/*.csproj"))
		{
			var projectFileSuffix = Regex.Replace(projectFile.GetFilenameWithoutExtension().ToString(), @"^Faithlife\.Tracing\.?", "");
			if (projectFileSuffix.Equals(projectSuffix, StringComparison.OrdinalIgnoreCase))
			{
				var projectPath = projectFile.FullPath;
				DotNetCorePack(projectPath, new DotNetCorePackSettings { Configuration = configuration, OutputDirectory = "release", VersionSuffix = versionSuffix });
			}
		}
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPackage")
	.Does(() =>
	{
		var nupkgPaths = GetFiles("release/*.nupkg").Select(x => x.FullPath).ToList();
		if (nupkgPaths.Count != 0)
		{
			// there should have been only one package created (for the current tag/trigger)
			var nupkgPath = nupkgPaths[0];

			// extract the nupkg version from the file path
			var versionMatch = Regex.Match(nupkgPath, @"\.([0-9]+\.[0-9]+\.[0-9]+(?:-[a-zA-Z0-9.]+)?)\.nupkg$");
			var version = versionMatch.Groups[1].ToString();

			var projectFileSuffix = Regex.Replace(System.IO.Path.GetFileName(nupkgPath.Replace(versionMatch.Groups[0].ToString(), "")), @"^Faithlife\.Tracing\.?", "");
			if (!string.IsNullOrEmpty(nugetApiKey) && Regex.IsMatch(trigger ?? "", "^(?:([a-z.]+)-)?v[0-9]"))
			{
				if (trigger != null && trigger != $"{(projectFileSuffix != "" ? (projectFileSuffix.ToLowerInvariant() + "-") : "")}v{version}")
					throw new InvalidOperationException($"Trigger '{trigger}' doesn't match package version '{version}'.");

				NuGetPush(nupkgPath, new NuGetPushSettings { ApiKey = nugetApiKey, Source = nugetSource });
			}
			else
			{
				Information("To publish this package, push this git tag: v" + version);
			}
		}
	});

Task("Default")
	.IsDependentOn("Test");

void ExecuteProcess(string exePath, string arguments)
{
	if (IsRunningOnUnix())
	{
		arguments = exePath + " " + arguments;
		exePath = "mono";
	}
	int exitCode = StartProcess(exePath, arguments);
	if (exitCode != 0)
		throw new InvalidOperationException($"{exePath} failed with exit code {exitCode}.");
}

RunTarget(target);
