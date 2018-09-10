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
			var operation = context.ActionDescriptor.AttributeRouteInfo?.Template ??
				routeData.Routers.OfType<Route>().Select(x => x.ParsedTemplate.TemplateText).FirstOrDefault();
            if (operation != null)
				requestTrace.SetTag(TraceTagNames.Operation, operation);

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
