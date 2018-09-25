using System;

namespace Faithlife.Tracing.AspNet
{
	/// <summary>
	/// An <see cref="ITraceSpanProvider"/> that has a "stack" of up to four trace spans: the request
	/// the controller action method, and any child actions; it returns the deepest span in the stack as
	/// the current one.
	/// </summary>
	internal sealed class RequestActionTraceSpanProvider : ITraceSpanProvider
	{
		public ITraceSpan CurrentSpan => m_spans[m_spanIndex];

		public RequestActionTraceSpanProvider(ITraceSpan requestSpan)
		{
			m_spans = new ITraceSpan[4];
			m_spans[0] = requestSpan ?? throw new ArgumentNullException(nameof(requestSpan));
		}

		public void StartActionSpan(string serviceName, string controller, string action)
		{
			if (m_spans[m_spanIndex] == null)
				throw new InvalidOperationException("Can't set action span when request span isn't present");
			if (m_spanIndex == m_spans.Length - 1)
				throw new InvalidOperationException("Maximum child action nesting depth exceeded");

			var childSpan = m_spans[m_spanIndex].StartChildSpan(TraceSpanKind.Server,
				new[]
				{
					(SpanTagNames.Service, serviceName),
					(SpanTagNames.Operation, controller + "." + action),
				});
			m_spans[++m_spanIndex] = childSpan;
		}

		public void FinishActionSpan() => PopSpan();
		public void FinishRequestSpan() => PopSpan();

		private void PopSpan()
		{
			m_spans[m_spanIndex]?.Dispose();
			m_spans[m_spanIndex] = null;
			if (m_spanIndex > 0)
				m_spanIndex--;
		}

		readonly ITraceSpan[] m_spans;
		int m_spanIndex;
	}
}
