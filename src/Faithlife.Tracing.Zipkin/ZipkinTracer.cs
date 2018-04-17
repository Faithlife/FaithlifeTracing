using System;
using System.Collections.Generic;
using zipkin4net;
using zipkin4net.Propagation;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace Faithlife.Tracing.Zipkin
{
	internal sealed class ZipkinTracer : ITracer
	{
		public ZipkinTracer(Uri collectorUri)
		{
			if (collectorUri == null)
				throw new ArgumentNullException(nameof(collectorUri));

			IZipkinSender sender;
			ISpanSerializer serializer;

			if (collectorUri.Scheme == "http" || collectorUri.Scheme == "https")
			{
				sender = new HttpZipkinSender(collectorUri.AbsoluteUri, "application/json");
				serializer = new JSONSpanSerializer();
			}
			else
			{
				throw new ArgumentException("Unrecognized Zipkin endpoint", nameof(collectorUri));
			}

			TraceManager.SamplingRate = 1.0f;
			TraceManager.Trace128Bits = true;
			var tracer = new zipkin4net.Tracers.Zipkin.ZipkinTracer(sender, serializer, new Statistics());
			TraceManager.RegisterTracer(tracer);
			TraceManager.Start(new NullLogger());
		}

		public ITrace ExtractTrace(Func<string, string> extractKey)
		{
			var traceContext = s_w3cExtractor.Extract(extractKey) ?? s_b3Extractor.Extract(extractKey);
			return traceContext == null ? null : new ZipkinTrace(this, Trace.CreateFromId(traceContext), TraceKind.Server);
		}

		public void InjectTrace(ITrace trace, Action<string, string> inject)
		{
			s_injector.Inject(((ZipkinTrace) trace).WrappedTrace.CurrentSpan, inject);
		}

		public ITrace StartTrace(ITrace parent, TraceKind kind, IEnumerable<(string Name, string Value)> tags)
		{
			var trace = (parent as ZipkinTrace)?.WrappedTrace.Child() ?? Trace.Create();
			if (kind == TraceKind.Client)
				trace.Record(Annotations.ClientSend());
			else if (kind == TraceKind.Server)
				trace.Record(Annotations.ServerRecv());
			var zipkinTrace = new ZipkinTrace(this, trace, kind);
			foreach (var (name, value) in tags)
			{
				if (kind == TraceKind.Local && name == TraceTagNames.Operation)
					trace.Record(Annotations.LocalOperationStart(value));
				else
					zipkinTrace.SetTag(name, value);
			}
			return zipkinTrace;
		}

		static readonly Getter<Func<string, string>, string> s_callbackGetter = (f, k) => f(k);
		static readonly IExtractor<Func<string, string>> s_w3cExtractor = W3CPropagation.Instance.Extractor(s_callbackGetter);
		static readonly IExtractor<Func<string, string>> s_b3Extractor = Propagations.B3String.Extractor(s_callbackGetter);
		static readonly IInjector<Action<string, string>> s_injector = W3CPropagation.Instance.Injector<Action<string, string>>((a, k, v) => a(k, v));
	}
}
