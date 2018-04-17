namespace Faithlife.Tracing
{
	/// <summary>
	/// Defines string constants for common span tags.
	/// </summary>
	/// <remarks>See <a href="https://github.com/opentracing/specification/blob/master/semantic_conventions.md">OpenTracing Semantic Conventions</a>.</remarks>
	public static class TraceTagNames
	{
		/// <summary>
		/// Database instance name.
		/// </summary>
		public const string DatabaseInstance = "db.instance";

		/// <summary>
		/// A database statement for the given database type.
		/// </summary>
		public const string DatabaseStatement = "db.statement";

		/// <summary>
		/// Database type. For any SQL database, <code>sql</code>. For others, the lower-case database category, e.g. <code>cassandra</code>, <code>hbase</code>, or <code>redis</code>.
		/// </summary>
		public const string DatabaseType = "db.type";

		/// <summary>
		/// HTTP response status code for the associated Span. 
		/// </summary>
		public const string HttpStatusCode = "http.status_code";

		/// <summary>
		/// The HTTP server host name.
		/// </summary>
		public const string HttpHost = "http.host";

		/// <summary>
		/// HTTP method of the request for the associated Span.
		/// </summary>
		public const string HttpMethod = "http.method";

		/// <summary>
		/// The path of the HTTP request.
		/// </summary>
		public const string HttpPath = "http.path";

		/// <summary>
		/// URL of the request being handled in this segment of the trace, in standard URI format.
		/// </summary>
		public const string HttpUrl = "http.url";

		/// <summary>
		/// Operation name, a human-readable string which concisely represents the work done by the Span.
		/// </summary>
		public const string Operation = "operation";

		/// <summary>
		/// The application or service generating this Span.
		/// </summary>
		public const string Service = "service";
	}
}
