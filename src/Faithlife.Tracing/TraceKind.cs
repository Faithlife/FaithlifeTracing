namespace Faithlife.Tracing
{
	/// <summary>
	/// Specifies the type of trace span being created.
	/// </summary>
	public enum TraceKind
	{
		/// <summary>
		/// The client side of an RPC.
		/// </summary>
		Client,

		/// <summary>
		/// The server side of an RPC.
		/// </summary>
		Server,

		/// <summary>
		/// A local operation.
		/// </summary>
		Local,
	}
}
