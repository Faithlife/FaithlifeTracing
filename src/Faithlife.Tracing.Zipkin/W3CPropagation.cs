using System;
using System.Collections.Generic;
using System.Globalization;
using zipkin4net;
using zipkin4net.Propagation;

namespace Faithlife.Tracing.Zipkin
{
	/// <summary>
	/// Propagates traces according to <a href="https://w3c.github.io/distributed-tracing/report-trace-context.html">W3C Distributed Tracing draft</a> (retrieved 2018-04-10).
	/// </summary>
	internal sealed class W3CPropagation : IPropagation<string>
	{
		public static W3CPropagation Instance { get; } = new W3CPropagation();

		public IInjector<TCarrier> Injector<TCarrier>(Setter<TCarrier, string> setter) => new W3CInjector<TCarrier>(setter);

		public IExtractor<TCarrier> Extractor<TCarrier>(Getter<TCarrier, string> getter) => new W3CExtractor<TCarrier>(getter);

		internal static (string Parent, string Context) CreateHeaderValues(ITraceContext traceContext)
		{
			var traceId = "00-" + traceContext.TraceIdHigh.ToString("x16") + traceContext.TraceId.ToString("x16") + "-" + traceContext.SpanId.ToString("x16") + "-" + ((traceContext.Sampled ?? false) ? "01" : "00");
			var previousContext = (traceContext.Extra == null || traceContext.Extra.Count == 0) ? null : (string) traceContext.Extra[0];
			var flags = (byte) ((traceContext.Debug ? 2 : 0) | (traceContext.ParentSpanId.HasValue ? 1 : 0));
			var context = c_vendorKeyEquals + flags.ToString("x2", CultureInfo.InvariantCulture) + (traceContext.ParentSpanId.HasValue ? traceContext.ParentSpanId.Value.ToString("x16") : "") + (previousContext == null ? "" : ("," + previousContext));
			return (traceId, context);
		}

		internal static ITraceContext Extract(string traceParent, string traceContext)
		{
			long? parentSpanId = null;
			bool isDebug = false;

			if (traceContext != null)
			{
				var index = traceContext.IndexOf(c_vendorKeyEquals, StringComparison.Ordinal);
				if (index != -1)
				{
					var valueIndex = index + c_vendorKeyEquals.Length;
					var commaIndex = traceContext.IndexOf(',', valueIndex);
					var valueLength = (commaIndex == -1 ? traceContext.Length : commaIndex) - valueIndex;
					if (valueLength >= 2)
					{
						var contextFlags = byte.Parse(traceContext.Substring(valueIndex, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
						isDebug = (contextFlags & 2) == 2;
						var hasParentSpanId = (contextFlags & 1) == 1;
						if (hasParentSpanId && valueLength >= 18)
							parentSpanId = long.Parse(traceContext.Substring(valueIndex + 2, 16), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
					}

					// remove our vendor string from tracecontext
					if (index == 0 && c_vendorKeyEquals.Length + valueLength == traceContext.Length)
					{
						traceContext = null;
					}
					else
					{
						var before = traceContext.Substring(0, index);
						var after = traceContext.Substring(valueIndex + valueLength);
						if (before.Length != 0 && after.Length == 0)
							before = before.Substring(0, before.Length - 1);
						if (after.Length != 0)
							after = after.Substring(1);
						traceContext = before + after;
					}
				}
			}

			var traceIdHigh = long.Parse(traceParent.Substring(3, 16), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var traceIdLow = long.Parse(traceParent.Substring(19, 16), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var spanId = long.Parse(traceParent.Substring(36, 16), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var flags = byte.Parse(traceParent.Substring(53, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var isSampled = ((flags & 0x1) == 0x1) ? true : default(bool?);
			return new W3CTraceContext(traceIdHigh, traceIdLow, spanId, parentSpanId, isSampled, isDebug, traceContext);
		}

		internal static bool IsValidTraceParent(string traceParent)
		{
			if (traceParent == null || traceParent.Length != 55)
				return false;

			if (traceParent[0] != '0' || traceParent[1] != '0' || traceParent[2] != '-')
				return false;

			for (int i = 3; i < traceParent.Length; i++)
			{
				var ch = traceParent[i];
				if (i == 35 || i == 52)
				{
					if (ch != '-')
						return false;
				}
				else
				{
					if (!((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')))
						return false;
				}
			}

			return true;
		}

		private sealed class W3CInjector<TCarrier> : IInjector<TCarrier>
		{
			public W3CInjector(Setter<TCarrier, string> setter) => m_setter = setter;

			public void Inject(ITraceContext traceContext, TCarrier carrier)
			{
				var (parent, context) = CreateHeaderValues(traceContext);
				m_setter(carrier, c_traceParentHeader, parent);
				m_setter(carrier, c_traceContextHeader, context);
			}

			readonly Setter<TCarrier, string> m_setter;
		}

		private sealed class W3CExtractor<TCarrier> : IExtractor<TCarrier>
		{
			public W3CExtractor(Getter<TCarrier, string> getter) => m_getter = getter;

			public ITraceContext Extract(TCarrier carrier)
			{
				var traceParent = m_getter(carrier, c_traceParentHeader);
				if (!IsValidTraceParent(traceParent))
					return null;

				var context = m_getter(carrier, c_traceContextHeader);
				return W3CPropagation.Extract(traceParent, context);
			}

			readonly Getter<TCarrier, string> m_getter;
		}

		internal sealed class W3CTraceContext : ITraceContext
		{
			public W3CTraceContext(long traceIdHigh, long traceIdLow, long spanId, long? parentSpanId, bool? isSampled, bool isDebug, string context)
			{
				TraceIdHigh = traceIdHigh;
				TraceId = traceIdLow;
				SpanId = spanId;
				ParentSpanId = parentSpanId;
				Sampled = isSampled;
				Debug = isDebug;
				if (context != null)
					Extra = new List<object> { context };
			}

			public long TraceIdHigh { get; }
			public long TraceId { get; }
			public long? ParentSpanId { get; }
			public long SpanId { get; }
			public bool? Sampled { get; }
			public bool Debug { get; }
			public List<object> Extra { get; }
		}

		const string c_vendorKeyEquals = "faithlifezipkin=";
		const string c_traceParentHeader = "traceparent";
		const string c_traceContextHeader = "tracecontext";
	}
}
