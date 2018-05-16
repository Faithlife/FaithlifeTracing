using System;
using System.Globalization;
using System.Threading;
using System.Web;

namespace Faithlife.Tracing.AspNet
{
	internal static class AspNetTracing
	{
		public static bool Initialize(HttpApplication application, string serviceName, Func<ITracer> createTracer, double? samplingRate)
		{
			var firstTimeInitialization = false;

			if (s_serviceName == null)
			{
				lock (s_lock)
				{
					if (s_serviceName == null)
					{
						if (serviceName == null)
							throw new ArgumentOutOfRangeException(nameof(serviceName));

						var actualSamplingRate = samplingRate ?? 1;
						SamplingRate =
							actualSamplingRate < 0 ? throw new ArgumentOutOfRangeException(nameof(samplingRate), "SamplingRate cannot be negative") :
							actualSamplingRate > 1 ? throw new ArgumentOutOfRangeException(nameof(samplingRate), "SamplingRate must be between 0.0 and 1.0") :
							(int) Math.Round(actualSamplingRate * c_samplingPrecision);
						Tracer = createTracer?.Invoke();
						s_serviceName = serviceName;

						firstTimeInitialization = true;
					}
				}
			}

			application.BeginRequest += BeginRequest;
			application.EndRequest += EndRequest;
			return firstTimeInitialization;
		}

		public static void BeginRequest(object sender, EventArgs e)
		{
			if (Tracer == null)
				return;

			var httpContext = ((HttpApplication) sender).Context;
			var headers = httpContext.Request.Headers;
			var parentTrace = Tracer.ExtractTrace(x => headers[x]);
			if (parentTrace != null || headers["X-B3-Flags"] == "1" || GetHashCode(Interlocked.Increment(ref s_requestCount)) % c_samplingPrecision < SamplingRate)
			{
				var trace = Tracer.StartTrace(parentTrace, TraceKind.Server,
					new[]
					{
						(TraceTagNames.Service, s_serviceName),
						(TraceTagNames.HttpHost, httpContext.Server.MachineName),
						(TraceTagNames.HttpMethod, httpContext.Request.HttpMethod),
						(TraceTagNames.HttpUrl, httpContext.Request.Url.OriginalString),
						(TraceTagNames.HttpPath, httpContext.Request.RawUrl),
					});
				var provider = new RequestActionTraceProvider(trace);
				HttpContext.Current.Items[c_providerKey] = provider;
			}
		}

		public static void EndRequest(object sender, EventArgs e)
		{
			var provider = GetProvider(((HttpApplication) sender).Context);
			if (provider != null)
			{
				provider.CurrentTrace.SetTag(TraceTagNames.HttpStatusCode, HttpContext.Current.Response.StatusCode.ToString(CultureInfo.InvariantCulture));
				provider.FinishRequestTrace();
			}
		}

		public static RequestActionTraceProvider GetProvider(HttpContextBase context) => (RequestActionTraceProvider) context?.Items[c_providerKey];
		public static RequestActionTraceProvider GetProvider(HttpContext context) => (RequestActionTraceProvider) context?.Items[c_providerKey];

		public static ITracer Tracer { get; set; }

		public static int SamplingRate { get; set; }

		// From http://burtleburtle.net/bob/hash/integer.html
		private static uint GetHashCode(int value)
		{
			unchecked
			{
				var n = (uint) value;
				n = (n + 0x7ed55d16) + (n << 12);
				n = (n ^ 0xc761c23c) ^ (n >> 19);
				n = (n + 0x165667b1) + (n << 5);
				n = (n + 0xd3a2646c) ^ (n << 9);
				n = (n + 0xfd7046c5) + (n << 3);
				n = (n ^ 0xb55a4f09) ^ (n >> 16);
				return n;
			}
		}

		static readonly object s_lock = new object();
		static string s_serviceName;
		static int s_requestCount;

		const string c_providerKey = "Faithlife.Tracing.AspNet.AspNetTracing.Provider";
		const int c_samplingPrecision = 65536;
	}
}
