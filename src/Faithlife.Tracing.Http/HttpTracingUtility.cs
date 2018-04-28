using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Faithlife.Tracing.Http
{
	public static class HttpTracingUtility
	{
		public static IDisposable StartHttpTrace(this ITraceProvider traceProvider, HttpWebRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			var requestTrace = SetHttpHeadersAndCreateTrace(traceProvider?.CurrentTrace, request.RequestUri, (k, v) => request.Headers[k] = v);
			requestTrace?.SetTag(TraceTagNames.HttpMethod, request.Method);
			return requestTrace;
		}

		public static HttpMessageHandler CreateHttpMessageHandler(ITraceProvider traceProvider, HttpMessageHandler messageHandler) => new TracingHttpMessageHandler(traceProvider, messageHandler);

		private sealed class TracingHttpMessageHandler : MessageProcessingHandler
		{
			public TracingHttpMessageHandler(ITraceProvider traceProvider, HttpMessageHandler innerHandler)
				: base(innerHandler)
			{
				m_traceProvider = traceProvider;
			}

			protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var currentTrace = m_traceProvider?.CurrentTrace;
				var requestTrace = SetHttpHeadersAndCreateTrace(currentTrace, request.RequestUri, (k, v) => request.Headers.Add(k, v));
				if (requestTrace != null)
				{
					requestTrace.SetTag(TraceTagNames.HttpMethod, request.Method.ToString());
					request.Properties[c_traceKey] = requestTrace;
				}
				return request;
			}

			protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
			{
				var trace = response.RequestMessage.Properties.TryGetValue(c_traceKey, out var t) ? (ITrace) t : null;
				if (trace != null)
				{
					trace.SetTag(TraceTagNames.HttpStatusCode, ((int) response.StatusCode).ToString());
					trace.Dispose();
				}
				return response;
			}

			const string c_traceKey = "Faithlife.Tracing.Http.TracingHttpMessageHandler.Trace";

			readonly ITraceProvider m_traceProvider;
		}

		private static ITrace SetHttpHeadersAndCreateTrace(ITrace currentTrace, Uri requestUri, Action<string, string> setHeader)
		{
			var requestTrace = currentTrace?.StartChildTrace(TraceKind.Client,
				new[]
				{
					(TraceTagNames.Service, requestUri.Authority),
					(TraceTagNames.Operation, requestUri.AbsolutePath),
					(TraceTagNames.HttpUrl, requestUri.AbsoluteUri),
				});
			requestTrace?.Tracer.InjectTrace(requestTrace, setHeader);
			return requestTrace;
		}
	}
}
