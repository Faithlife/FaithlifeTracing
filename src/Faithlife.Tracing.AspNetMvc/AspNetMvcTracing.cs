using System;
using System.Web;
using System.Web.Mvc;
using Faithlife.Tracing.AspNet;

namespace Faithlife.Tracing.AspNetMvc
{
	public static class AspNetMvcTracing
	{
		public static void Initialize(HttpApplication application, AspNetMvcTracingSettings settings)
		{
			if (application == null)
				throw new ArgumentNullException(nameof(application));
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			if (AspNetTracing.Initialize(application, settings.ServiceName, settings.CreateTracer, settings.SamplingRate))
			{
				GlobalFilters.Filters.Add(new TracingActionFilterAttribute(settings.ServiceName));
			}
		}

		public static ITraceSpanProvider GetProvider(HttpContext httpContext) => AspNetTracing.GetProvider(httpContext) ?? NoopTraceSpanProvider.Instance;
	}
}
