using System;
using System.Collections.Generic;

namespace Faithlife.Tracing
{
	/// <summary>
	/// <see cref="ITracer"/> can create new spans and serialize/deserialize them.
	/// </summary>
	/// <remarks>See <a href="https://github.com/opentracing/specification/blob/master/specification.md">OpenTracing specification</a> for more details.</remarks>
	public interface ITracer
	{
		/// <summary>
		/// Creates a new span (that is optionally a child of an existing span).
		/// </summary>
		/// <param name="parent">The parent span of the new span. This parameter may be <c>null</c> to create a new trace.</param>
		/// <param name="kind">The <see cref="TraceKind"/>.</param>
		/// <param name="tags">A sequence of span tags; see <see cref="ITrace.SetTag"/>.</param>
		/// <returns>A new <see cref="ITrace"/>.</returns>
		ITrace StartTrace(ITrace parent, TraceKind kind, IEnumerable<(string Name, string Value)> tags);

		/// <summary>
		/// Extracts a trace from a string-to-string map.
		/// </summary>
		/// <param name="extractKey">A <see cref="Func{T,TResult}"/> that looks up a value given a string key.</param>
		/// <returns>A new <see cref="ITrace"/> if one could be extracted successfully, or <c>null</c>.</returns>
		ITrace ExtractTrace(Func<string, string> extractKey);

		/// <summary>
		/// Injects a trace into a string-to-string map.
		/// </summary>
		/// <param name="trace">The <see cref="ITrace"/> to inject.</param>
		/// <param name="inject">A <see cref="Action{T1,T2}"/> that sets a key to a specified value.</param>
		void InjectTrace(ITrace trace, Action<string, string> inject);
	}
}
