using System;
using System.Web;
using System.Web.Http;
using Faithlife.Tracing.AspNet;

namespace Faithlife.Tracing.AspNetWebApi
{
	public static class AspNetWebApiTracing
	{
		public static void Initialize(HttpApplication application, AspNetWebApiTracingSettings settings)
		{
			if (application == null)
				throw new ArgumentNullException(nameof(application));
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			if (AspNetTracing.Initialize(application, settings.ServiceName, settings.CreateTracer, settings.SamplingRate))
			{
				GlobalConfiguration.Configuration.Filters.Add(new TracingActionFilterAttribute(settings.ServiceName));
			}
		}

		public static ITraceProvider GetProvider(HttpContext context) => AspNetTracing.GetProvider(context) ?? NullTraceProvider.Instance;
	}
}
