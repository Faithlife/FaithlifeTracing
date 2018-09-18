using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Faithlife.Tracing.Http
{
	public static class HttpTracingUtility
	{
		public static IDisposable StartHttpSpan(this ITraceSpanProvider traceSpanProvider, HttpWebRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			var requestSpan = SetHttpHeadersAndCreateSpan(traceSpanProvider?.CurrentSpan, request.RequestUri, (k, v) => request.Headers[k] = v);
			requestSpan?.SetTag(SpanTagNames.HttpMethod, request.Method);
			return requestSpan;
		}

		public static HttpMessageHandler CreateHttpMessageHandler(ITraceSpanProvider traceSpanProvider, HttpMessageHandler messageHandler) => new TracingHttpMessageHandler(traceSpanProvider, messageHandler);

		private sealed class TracingHttpMessageHandler : MessageProcessingHandler
		{
			public TracingHttpMessageHandler(ITraceSpanProvider traceSpanProvider, HttpMessageHandler innerHandler)
				: base(innerHandler)
			{
				m_traceSpanProvider = traceSpanProvider;
			}

			protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var currentSpan = m_traceSpanProvider?.CurrentSpan;
				var requestSpan = SetHttpHeadersAndCreateSpan(currentSpan, request.RequestUri, (k, v) => request.Headers.Add(k, v));
				if (requestSpan != null)
				{
					requestSpan.SetTag(SpanTagNames.HttpMethod, request.Method.ToString());
					request.Properties[c_spanKey] = requestSpan;
				}
				return request;
			}

			protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
			{
				var span = response.RequestMessage.Properties.TryGetValue(c_spanKey, out var t) ? (ITraceSpan) t : null;
				if (span != null)
				{
					span.SetTag(SpanTagNames.HttpStatusCode, ((int) response.StatusCode).ToString());
					span.Dispose();
				}
				return response;
			}

			const string c_spanKey = "Faithlife.Tracing.Http.TracingHttpMessageHandler.Span";

			readonly ITraceSpanProvider m_traceSpanProvider;
		}

		private static ITraceSpan SetHttpHeadersAndCreateSpan(ITraceSpan currentSpan, Uri requestUri, Action<string, string> setHeader)
		{
			var requestSpan = currentSpan?.StartChildSpan(TraceSpanKind.Client,
				new[]
				{
					(SpanTagNames.Service, requestUri.Authority),
					(SpanTagNames.Operation, requestUri.AbsolutePath),
					(SpanTagNames.HttpUrl, requestUri.AbsoluteUri),
				});
			requestSpan?.Tracer.InjectSpan(requestSpan, setHeader);
			return requestSpan;
		}
	}
}
