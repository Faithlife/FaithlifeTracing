namespace Faithlife.Tracing
{
	/// <summary>
	/// <see cref="ITraceSpanProvider"/> is implemented by an object that stores the current trace (for the active request, etc.) and can return it.
	/// </summary>
	/// <remarks>An <see cref="ITraceSpan"/> is an <see cref="ITraceSpanProvider"/> that returns itself.</remarks>
	public interface ITraceSpanProvider
	{
		/// <summary>
		/// The current trace.
		/// </summary>
		ITraceSpan CurrentSpan { get; }
	}
}
