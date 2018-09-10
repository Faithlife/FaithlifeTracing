using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Faithlife.Tracing.AspNetCore
{
	internal sealed class TracingActionFilterAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var traceProvider = AspNetCoreTracing.GetRequestActionTraceProvider(context.HttpContext);
			var requestTrace = traceProvider?.CurrentTrace;
			if (requestTrace == null)
				return;

			var routeData = context.HttpContext.GetRouteData();
			var route = routeData.Routers.OfType<Route>().FirstOrDefault();
			if (route != null)
				requestTrace.SetTag(TraceTagNames.Operation, route.ParsedTemplate.TemplateText);

			var serviceName = AspNetCoreTracing.GetServiceName(context.HttpContext);
			traceProvider.StartActionTrace(serviceName, (string) routeData.Values["controller"], (string) routeData.Values["action"]);

			base.OnActionExecuting(context);
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			AspNetCoreTracing.GetRequestActionTraceProvider(context.HttpContext)?.FinishActionTrace();
			base.OnActionExecuted(context);
		}
	}
}
