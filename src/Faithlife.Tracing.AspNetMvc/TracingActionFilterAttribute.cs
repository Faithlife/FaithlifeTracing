using System.Web.Mvc;
using System.Web.Routing;
using Faithlife.Tracing.AspNet;

namespace Faithlife.Tracing.AspNetMvc
{
	internal sealed class TracingActionFilterAttribute : ActionFilterAttribute
	{
		public TracingActionFilterAttribute(string serviceName)
		{
			m_serviceName = serviceName;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var provider = AspNetTracing.GetProvider(filterContext.HttpContext);
			var requestSpan = provider?.CurrentSpan;
			if (requestSpan == null)
				return;

			var routeData = filterContext.Controller.ControllerContext.RouteData;
			if (routeData.Route is Route route && !filterContext.IsChildAction)
				requestSpan.SetTag(SpanTagNames.Operation, route.Url);

			provider.StartActionSpan(m_serviceName, (string) routeData.Values["controller"], (string) routeData.Values["action"]);

			base.OnActionExecuting(filterContext);
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			AspNetTracing.GetProvider(filterContext.HttpContext)?.FinishActionSpan();
			base.OnActionExecuted(filterContext);
		}

		private readonly string m_serviceName;
	}
}
