using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Faithlife.Tracing.AspNetCore
{
	internal sealed class TracingActionFilterAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var provider = AspNetCoreTracing.GetRequestActionTraceSpanProvider(context.HttpContext);
			var requestSpan = provider?.CurrentSpan;
			if (requestSpan == null)
				return;

			var routeData = context.HttpContext.GetRouteData();
			var operation = context.ActionDescriptor.AttributeRouteInfo?.Template ??
				routeData.Routers.OfType<Route>().Select(x => x.ParsedTemplate.TemplateText).FirstOrDefault();
            if (operation != null)
				requestSpan.SetTag(SpanTagNames.Operation, operation);

			var serviceName = AspNetCoreTracing.GetServiceName(context.HttpContext);
			provider.StartActionSpan(serviceName, (string) routeData.Values["controller"], (string) routeData.Values["action"]);

			base.OnActionExecuting(context);
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			AspNetCoreTracing.GetRequestActionTraceSpanProvider(context.HttpContext)?.FinishActionSpan();
			base.OnActionExecuted(context);
		}
	}
}
