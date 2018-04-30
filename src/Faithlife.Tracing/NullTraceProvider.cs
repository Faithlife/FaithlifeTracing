namespace Faithlife.Tracing
{
	/// <summary>
	/// An <see cref="ITraceProvider"/> that returns <c>null</c> for <c>CurrentTrace</c>.
	/// </summary>
	public sealed class NullTraceProvider : ITraceProvider
	{
		/// <summary>
		/// An instance of <see cref="NullTraceProvider"/>.
		/// </summary>
		public static ITraceProvider Instance { get; } = new NullTraceProvider();

		/// <summary>
		/// The current trace, which will always be <c>null</c>.
		/// </summary>
		public ITrace CurrentTrace => null;
	}
}
