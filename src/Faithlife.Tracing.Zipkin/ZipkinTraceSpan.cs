using zipkin4net;

namespace Faithlife.Tracing.Zipkin
{
	internal sealed class ZipkinTraceSpan : ITraceSpan
	{
		public ZipkinTraceSpan(ZipkinTracer tracer, Trace trace, TraceSpanKind kind)
		{
			Tracer = tracer;
			m_kind = kind;
			WrappedTrace = trace;
		}

		public ITraceSpan CurrentSpan => this;

		public void Dispose()
		{
			WrappedTrace.Record(m_kind == TraceSpanKind.Client ? Annotations.ClientRecv() :
				m_kind == TraceSpanKind.Server ? Annotations.ServerSend() : Annotations.LocalOperationStop());
		}

		public void SetTag(string name, string value)
		{
			if (name == SpanTagNames.Service)
				WrappedTrace.Record(Annotations.ServiceName(Truncate(value, 128)));
			else if (name == SpanTagNames.Operation)
				WrappedTrace.Record(Annotations.Rpc(Truncate(value, 128)));
			else
				WrappedTrace.Record(Annotations.Tag(name, value));
		}

		public ITracer Tracer { get; }
		public string TraceId => WrappedTrace.CurrentSpan.TraceIdHigh.ToString("x16") + WrappedTrace.CurrentSpan.TraceId.ToString("x16");
		public string SpanId => WrappedTrace.CurrentSpan.SpanId.ToString("x16");

		private static string Truncate(string value, int maxLength) => value == null ? null : value.Length < maxLength ? value : value.Substring(0, maxLength);

		internal Trace WrappedTrace { get; }

		readonly TraceSpanKind m_kind;
	}
}
