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
			var traceProvider = AspNetTracing.GetProvider(HttpContext.Current);
			var requestTrace = traceProvider?.CurrentTrace;
			if (requestTrace == null)
				return;

			requestTrace.SetTag(TraceTagNames.Operation, actionContext.ControllerContext.RouteData.Route.RouteTemplate);
			traceProvider.StartActionTrace(m_serviceName, actionContext.ControllerContext.ControllerDescriptor.ControllerName, actionContext.ActionDescriptor.ActionName);

			base.OnActionExecuting(actionContext);
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			AspNetTracing.GetProvider(HttpContext.Current)?.FinishActionTrace();
			base.OnActionExecuted(actionExecutedContext);
		}

		readonly string m_serviceName;
	}
}
