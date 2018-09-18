namespace Faithlife.Tracing
{
	/// <summary>
	/// An <see cref="ITraceSpanProvider"/> that returns <c>null</c> for <c>CurrentTrace</c>.
	/// </summary>
	public sealed class NullTraceSpanProvider : ITraceSpanProvider
	{
		/// <summary>
		/// An instance of <see cref="NullTraceSpanProvider"/>.
		/// </summary>
		public static ITraceSpanProvider Instance { get; } = new NullTraceSpanProvider();

		/// <summary>
		/// The current trace, which will always be <c>null</c>.
		/// </summary>
		public ITraceSpan CurrentSpan => null;
	}
}
