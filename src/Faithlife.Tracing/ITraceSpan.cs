using System;

namespace Faithlife.Tracing
{
	/// <summary>
	/// <see cref="ITraceSpan"/> represents a span in a trace.
	/// </summary>
	/// <remarks>See <a href="https://github.com/opentracing/specification/blob/master/specification.md">OpenTracing Specification</a> for definitions of trace, span, etc.</remarks>
	public interface ITraceSpan : ITraceSpanProvider, IDisposable
	{
		/// <summary>
		/// Sets a tag on this span.
		/// </summary>
		/// <param name="name">The tag name. This may be a value taken from <see cref="SpanTagNames"/>.</param>
		/// <param name="value">The value of the tag.</param>
		void SetTag(string name, string value);

		/// <summary>
		/// The <see cref="ITracer"/> that created this <see cref="ITraceSpan"/>.
		/// </summary>
		ITracer Tracer { get; }

		/// <summary>
		/// The trace ID. This is the same for all <see cref="ITraceSpan"/> objects that are part of the same trace.
		/// </summary>
		string TraceId { get; }

		/// <summary>
		/// The span ID. This is unique for each <see cref="ITraceSpan"/> object.
		/// </summary>
		string SpanId { get; }
	}
}
