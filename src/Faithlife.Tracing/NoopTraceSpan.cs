using System;

namespace Faithlife.Tracing
{
	/// <summary>
	/// An <see cref="ITraceSpan"/> that does not do any actual tracing.
	/// </summary>
	public sealed class NoopTraceSpan : ITraceSpan
	{
		/// <summary>
		/// An instance of <see cref="NoopTraceSpan"/>.
		/// </summary>
		public static ITraceSpan Instance { get; } = new NoopTraceSpan();

		ITracer ITraceSpan.Tracer => NoopTracer.Instance;

		string ITraceSpan.TraceId => string.Empty;

		string ITraceSpan.SpanId => string.Empty;

		ITraceSpan ITraceSpanProvider.CurrentSpan => this;

		void IDisposable.Dispose() { }

		void ITraceSpan.SetTag(string name, string value) { }
	}
}
