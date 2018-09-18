using System;

namespace Faithlife.Tracing.AspNet
{
	/// <summary>
	/// An <see cref="ITraceSpanProvider"/> that has a "stack" of two trace spans: the request
	/// and the controller action method; it returns the deepest span in the stack as
	/// the current one.
	/// </summary>
	internal sealed class RequestActionTraceSpanProvider : ITraceSpanProvider
	{
		public ITraceSpan CurrentSpan => m_actionSpan ?? m_requestSpan;

		public RequestActionTraceSpanProvider(ITraceSpan requestSpan)
		{
			m_requestSpan = requestSpan ?? throw new ArgumentNullException(nameof(requestSpan));
		}

		public void StartActionSpan(string serviceName, string controller, string action)
		{
			if (m_requestSpan == null)
				throw new InvalidOperationException("Can't set action span when request span isn't present");
			if (m_actionSpan != null)
				throw new InvalidOperationException("There is already a current action span");

			m_actionSpan = m_requestSpan.StartChildSpan(TraceSpanKind.Server,
				new[]
				{
					(SpanTagNames.Service, serviceName),
					(SpanTagNames.Operation, controller + "." + action),
				});
		}

		public void FinishActionSpan() => FinishSpan(ref m_actionSpan);
		public void FinishRequestSpan() => FinishSpan(ref m_requestSpan);

		private static void FinishSpan(ref ITraceSpan traceSpan)
		{
			traceSpan?.Dispose();
			traceSpan = null;
		}

		ITraceSpan m_requestSpan;
		ITraceSpan m_actionSpan;
	}
}
