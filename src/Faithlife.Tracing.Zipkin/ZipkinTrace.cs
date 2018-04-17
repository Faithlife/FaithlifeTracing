using zipkin4net;

namespace Faithlife.Tracing.Zipkin
{
	internal sealed class ZipkinTrace : ITrace
	{
		public ZipkinTrace(ZipkinTracer tracer, Trace trace, TraceKind traceKind)
		{
			Tracer = tracer;
			m_traceKind = traceKind;
			WrappedTrace = trace;
		}

		public ITrace CurrentTrace => this;

		public void Dispose()
		{
			WrappedTrace.Record(m_traceKind == TraceKind.Client ? Annotations.ClientRecv() :
				m_traceKind == TraceKind.Server ? Annotations.ServerSend() : Annotations.LocalOperationStop());
		}

		public void SetTag(string name, string value)
		{
			if (name == TraceTagNames.Service)
				WrappedTrace.Record(Annotations.ServiceName(Truncate(value, 128)));
			else if (name == TraceTagNames.Operation)
				WrappedTrace.Record(Annotations.Rpc(Truncate(value, 128)));
			else
				WrappedTrace.Record(Annotations.Tag(name, value));
		}

		public ITracer Tracer { get; }
		public string TraceId => WrappedTrace.CurrentSpan.TraceIdHigh.ToString("x16") + WrappedTrace.CurrentSpan.TraceId.ToString("x16");
		public string SpanId => WrappedTrace.CurrentSpan.SpanId.ToString("x16");

		private static string Truncate(string value, int maxLength) => value == null ? null : value.Length < maxLength ? value : value.Substring(0, maxLength);

		internal Trace WrappedTrace { get; }

		readonly TraceKind m_traceKind;
	}
}
