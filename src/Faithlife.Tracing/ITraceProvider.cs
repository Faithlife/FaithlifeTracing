namespace Faithlife.Tracing
{
	/// <summary>
	/// <see cref="ITraceProvider"/> is implemented by an object that stores the current trace (for the active request, etc.) and can return it.
	/// </summary>
	/// <remarks>An <see cref="ITrace"/> is an <see cref="ITraceProvider"/> that returns itself.</remarks>
	public interface ITraceProvider
	{
		/// <summary>
		/// The current trace.
		/// </summary>
		ITrace CurrentTrace { get; }
	}
}
