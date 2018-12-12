using System;
using System.Globalization;
using System.Threading;
using Faithlife.Tracing.AspNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Faithlife.Tracing.AspNetCore
{
	public static class AspNetCoreTracing
	{
		public static void UseTracing(this IApplicationBuilder app, Action<AspNetCoreTracingSettings> configure)
		{
			var settings = new AspNetCoreTracingSettings();
			configure(settings);

			var serviceName = settings.ServiceName ?? throw new ArgumentOutOfRangeException(nameof(settings.ServiceName));

			var actualSamplingRate = settings.SamplingRate ?? 1;
			SamplingRate =
				actualSamplingRate < 0 ? throw new ArgumentOutOfRangeException(nameof(settings.SamplingRate), "SamplingRate cannot be negative") :
				actualSamplingRate > 1 ? throw new ArgumentOutOfRangeException(nameof(settings.SamplingRate), "SamplingRate must be between 0.0 and 1.0") :
				(int) Math.Round(actualSamplingRate * c_samplingPrecision);
			Tracer = settings.CreateTracer?.Invoke();

			app.Use(async (httpContext, next) =>
			{
				var headers = httpContext.Request.Headers;
				var parentSpan = Tracer.ExtractSpan(x => headers[x]);
				if (parentSpan != null || headers["X-B3-Flags"] == "1" || GetHashCode(Interlocked.Increment(ref s_requestCount)) % c_samplingPrecision < SamplingRate)
				{
					var span = Tracer.StartSpan(parentSpan, TraceSpanKind.Server,
						new[]
						{
							(SpanTagNames.Service, serviceName),
							(SpanTagNames.HttpHost, httpContext.Request.Host.ToString()),
							(SpanTagNames.HttpMethod, httpContext.Request.Method),
							(SpanTagNames.HttpUrl, httpContext.Request.GetEncodedUrl()),
							(SpanTagNames.HttpPath, httpContext.Request.GetEncodedPathAndQuery()),
						});

					httpContext.Items[c_serviceNameKey] = serviceName;

					var provider = new RequestActionTraceSpanProvider(span);
					httpContext.Items[c_providerKey] = provider;

					try
					{
						await next();
					}
					finally
					{
						provider.CurrentSpan.SetTag(SpanTagNames.HttpStatusCode, httpContext.Response.StatusCode.ToString(CultureInfo.InvariantCulture));
						provider.FinishRequestSpan();
					}
				}
				else
				{
					await next();
				}
			});
		}

		public static IMvcBuilder AddTracing(this IMvcBuilder builder)
		{
			builder.AddMvcOptions(options => options.Filters.Add<TracingActionFilterAttribute>());
			return builder;
		}

		public static ITraceSpanProvider GetProvider(HttpContext context) => GetRequestActionTraceSpanProvider(context) ?? NoopTraceSpanProvider.Instance;

		internal static ITracer Tracer { get; set; }

		internal static int SamplingRate { get; set; }

		internal static string GetServiceName(HttpContext context) => (string) context?.Items[c_serviceNameKey];

		internal static RequestActionTraceSpanProvider GetRequestActionTraceSpanProvider(HttpContext context) => (RequestActionTraceSpanProvider) context?.Items[c_providerKey];

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

		static int s_requestCount;

		const string c_providerKey = "Faithlife.Tracing.AspNetCore.AspNetTracing.Provider";
		const string c_serviceNameKey = "Faithlife.Tracing.AspNetCore.AspNetTracing.ServiceName";
		const int c_samplingPrecision = 65536;
	}
}
