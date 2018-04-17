using System;

namespace Faithlife.Tracing.AspNet
{
	/// <summary>
	/// An <see cref="ITraceProvider"/> that has a "stack" of two traces: the request
	/// and the controller action method; it returns the deepest trace in the stack as
	/// the current one.
	/// </summary>
	internal sealed class RequestActionTraceProvider : ITraceProvider
	{
		public ITrace CurrentTrace => m_actionTrace ?? m_requestTrace;

		public RequestActionTraceProvider(ITrace requestTrace)
		{
			m_requestTrace = requestTrace ?? throw new ArgumentNullException(nameof(requestTrace));
		}

		public void StartActionTrace(string serviceName, string controller, string action)
		{
			if (m_requestTrace == null)
				throw new InvalidOperationException("Can't set action trace when request trace isn't present");
			if (m_actionTrace != null)
				throw new InvalidOperationException("There is already a current action trace");

			m_actionTrace = m_requestTrace.StartChildTrace(TraceKind.Server,
				new[]
				{
					(TraceTagNames.Service, serviceName),
					(TraceTagNames.Operation, controller + "." + action),
				});
		}

		public void FinishActionTrace() => FinishTrace(ref m_actionTrace);
		public void FinishRequestTrace() => FinishTrace(ref m_requestTrace);

		private static void FinishTrace(ref ITrace trace)
		{
			trace?.Dispose();
			trace = null;
		}

		ITrace m_requestTrace;
		ITrace m_actionTrace;
	}
}
