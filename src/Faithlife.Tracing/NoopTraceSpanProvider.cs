namespace Faithlife.Tracing
{
	/// <summary>
	/// An <see cref="ITraceSpanProvider"/> that does not do any actual tracing.
	/// </summary>
	public sealed class NoopTraceSpanProvider : ITraceSpanProvider
	{
		/// <summary>
		/// An instance of <see cref="NoopTraceSpanProvider"/>.
		/// </summary>
		public static ITraceSpanProvider Instance { get; } = new NoopTraceSpanProvider();

		ITraceSpan ITraceSpanProvider.CurrentSpan => NoopTraceSpan.Instance;
	}
}
