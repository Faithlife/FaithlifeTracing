using System;
using System.Collections.Generic;

namespace Faithlife.Tracing
{
	/// <summary>
	/// An <see cref="ITracer"/> that does not do any actual tracing.
	/// </summary>
	public sealed class NoopTracer : ITracer
	{
		/// <summary>
		/// An instance of <see cref="NoopTracer"/>.
		/// </summary>
		public static ITracer Instance { get; } = new NoopTracer();

		ITraceSpan ITracer.ExtractSpan(Func<string, string> extractKey) => NoopTraceSpan.Instance;

		void ITracer.InjectSpan(ITraceSpan traceSpan, Action<string, string> inject) { }

		ITraceSpan ITracer.StartSpan(ITraceSpan parent, TraceSpanKind kind, IEnumerable<(string Name, string Value)> tags) => NoopTraceSpan.Instance;
	}
}
