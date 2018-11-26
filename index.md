# Faithlife.Tracing

**Faithlife.Tracing** provides distributed tracing for .NET applications and web services.

## Installation

Faithlife.Tracing should be installed [via NuGet](https://www.nuget.org/packages/Faithlife.Tracing).

## Documentation

* [Reference Documentation](Faithlife.Tracing.md)

## How to Use

### ASP.NET

Install the `Faithlife.Tracing.AspNetMvc` or `Faithlife.Tracing.AspNetWebApi` package, as appropriate,
as well as `Faithlife.Tracing.Zipkin`. Then add the following code to the constructor of your
`HttpApplication`-derived class (in `Global.asax.cs`):

```
public MyApplication()
{
    AspNetWebApiTracing.Initialize(this, new AspNetWebApiTracingSettings // use Mvc instead of WebApi as appropriate
    {
        ServiceName = "MyService",
        CreateTracer = () => ZipkinTracing.CreateTracer(new Uri("http://zipkin-collector:9411")),
    });
}
```

In your type registry, register `ITraceSpanProvider` as a per-request object that's obtained
from `HttpContext.Current`. This may look something like (StructureMap):

    For<ITraceSpanProvider>().HttpContextScoped().Use(c => AspNetWebApiTracing.GetProvider(HttpContext.Current));

or (AutoFac):

    builder.Register<ITraceSpanProvider>(c => AspNetWebApiTracing.GetProvider(HttpContext.Current)).InstancePerRequest();

### ASP.NET Core

Install the `Faithlife.Tracing.AspNetCore` and `Faithlife.Tracing.Zipkin` packages. Add the following to your `Setup` class:

```
public void ConfigureServices(IServiceCollection services)
{
    // ...

    // If using MVC, add tracing support
    services.AddMvc().AddTracing();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    // ...

    app.UseTracing(settings =>
    {
        settings.ServiceName = "MyService";
        settings.CreateTracer = () => ZipkinTracing.CreateTracer(new Uri("http://zipkin-collector:9411"));
    });

    // Add tracing before MVC, if using
    app.UseMvcWithDefaultRoute();
}
```

### ADO.NET

Install the `Faithlife.Tracing.Data` package. Modify your code that creates a `DbConnection` object (e.g.,
`MySqlConnection`) to return a `TracingDbConnection`:

```
public DbConnection CreateConnection(string connectionString)
{
    ITraceSpanProvider traceProvider = // get the ITraceSpanProvider for the current request
    return TracingDbConnection.Create(new MySqlConnection(connectionString), traceProvider);
}
```

### HttpClient

Install the `Faithlife.Tracing.Http` package. Trace HTTP requests by inserting a new `HttpMessageHandler` into the stack.

```
ITraceSpanProvider traceSpanProvider = // get the ITraceSpanProvider for the current request
var tracingMessageHandler = HttpTracingUtility.CreateHttpMessageHandler(traceSpanProvider, new HttpClientHandler());
var httpClient = new HttpClient(tracingMessageHandler);
// use httpClient
```
