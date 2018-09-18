using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Faithlife.Tracing.AspNet;

namespace Faithlife.Tracing.AspNetWebApi
{
	internal sealed class TracingActionFilterAttribute : ActionFilterAttribute
	{
		public TracingActionFilterAttribute(string serviceName)
		{
			m_serviceName = serviceName;
		}

		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			var provider = AspNetTracing.GetProvider(HttpContext.Current);
			var requestSpan = provider?.CurrentSpan;
			if (requestSpan == null)
				return;

			requestSpan.SetTag(SpanTagNames.Operation, actionContext.ControllerContext.RouteData.Route.RouteTemplate);
			provider.StartActionSpan(m_serviceName, actionContext.ControllerContext.ControllerDescriptor.ControllerName, actionContext.ActionDescriptor.ActionName);

			base.OnActionExecuting(actionContext);
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			AspNetTracing.GetProvider(HttpContext.Current)?.FinishActionSpan();
			base.OnActionExecuted(actionExecutedContext);
		}

		readonly string m_serviceName;
	}
}
