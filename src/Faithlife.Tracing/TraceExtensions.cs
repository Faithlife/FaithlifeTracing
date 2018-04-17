using System.Collections.Generic;

namespace Faithlife.Tracing
{
	/// <summary>
	/// Provides helper methods for working with <see cref="ITrace"/> and <see cref="ITracer"/>.
	/// </summary>
	public static class TraceExtensions
	{
		/// <summary>
		/// Starts a new <see cref="ITrace"/> that is a child of <paramref name="trace"/>.
		/// </summary>
		/// <param name="trace">(Optional) The current trace. If this is <c>null</c>, this method returns <c>null</c>.</param>
		/// <param name="kind">The <see cref="TraceKind"/>.</param>
		/// <param name="tags">A sequence of span tags; see <see cref="ITrace.SetTag"/>.</param>
		/// <returns>A new <see cref="ITrace"/>, or <c>null</c>.</returns>
		public static ITrace StartChildTrace(this ITrace trace, TraceKind kind, IEnumerable<(string Name, string Value)> tags) =>
			trace?.Tracer.StartTrace(trace, kind, tags);

		/// <summary>
		/// Starts a new trace (that is the root of a trace hierarchy).
		/// </summary>
		/// <param name="tracer">The <see cref="ITracer"/>.</param>
		/// <param name="kind">The <see cref="TraceKind"/>.</param>
		/// <param name="tags">A sequence of span tags; see <see cref="ITrace.SetTag"/>.</param>
		/// <returns>A new <see cref="ITrace"/>.</returns>
		public static ITrace StartNewTrace(this ITracer tracer, TraceKind kind, IEnumerable<(string Name, string Value)> tags) =>
			tracer?.StartTrace(null, kind, tags);
	}
}
