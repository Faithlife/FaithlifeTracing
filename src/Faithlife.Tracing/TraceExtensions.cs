using System.Collections.Generic;

namespace Faithlife.Tracing
{
	/// <summary>
	/// Provides helper methods for working with <see cref="ITraceSpan"/> and <see cref="ITracer"/>.
	/// </summary>
	public static class TraceExtensions
	{
		/// <summary>
		/// Starts a new <see cref="ITraceSpan"/> that is a child of <paramref name="traceSpan"/>.
		/// </summary>
		/// <param name="traceSpan">(Optional) The current span. If this is <c>null</c>, this method returns <c>null</c>.</param>
		/// <param name="kind">The <see cref="TraceSpanKind"/>.</param>
		/// <param name="tags">A sequence of span tags; see <see cref="ITraceSpan.SetTag"/>.</param>
		/// <returns>A new <see cref="ITraceSpan"/>, or <c>null</c>.</returns>
		public static ITraceSpan StartChildSpan(this ITraceSpan traceSpan, TraceSpanKind kind, IEnumerable<(string Name, string Value)> tags) =>
			traceSpan?.Tracer.StartSpan(traceSpan, kind, tags);

		/// <summary>
		/// Starts a new span (that is the root of a trace hierarchy).
		/// </summary>
		/// <param name="tracer">The <see cref="ITracer"/>.</param>
		/// <param name="kind">The <see cref="TraceSpanKind"/>.</param>
		/// <param name="tags">A sequence of span tags; see <see cref="ITraceSpan.SetTag"/>.</param>
		/// <returns>A new <see cref="ITraceSpan"/>.</returns>
		public static ITraceSpan StartNewTrace(this ITracer tracer, TraceSpanKind kind, IEnumerable<(string Name, string Value)> tags) =>
			tracer?.StartSpan(null, kind, tags);
	}
}
