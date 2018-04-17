using zipkin4net;

namespace Faithlife.Tracing.Zipkin
{
	internal sealed class NullLogger : ILogger
	{
		public void LogInformation(string message)
		{
		}

		public void LogWarning(string message)
		{
		}

		public void LogError(string message)
		{
		}
	}
}
