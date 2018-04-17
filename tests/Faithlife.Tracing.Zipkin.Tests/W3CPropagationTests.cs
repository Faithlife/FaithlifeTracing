using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace Faithlife.Tracing.Zipkin.Tests
{
	[TestFixture]
	public class W3CPropagationTests
	{
		[TestCase(null, false)]
		[TestCase("", false)]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-0", false)]
		[TestCase("00-Xbf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", false)]
		[TestCase("01-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", false)]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736100f067aa0ba902b7-00", false)]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", true)]
		public void IsValidTraceParent(string traceParent, bool expected)
		{
			Assert.AreEqual(expected, W3CPropagation.IsValidTraceParent(traceParent));
		}

		[TestCase("4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, false, false, null, "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", "faithlifezipkin=00")]
		[TestCase("4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, true, false, null, "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=00")]
		[TestCase("4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", "b9c7c989f97918e1", true, false, null, "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=01b9c7c989f97918e1")]
		[TestCase("4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, true, true, null, "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=02")]
		[TestCase("0af7651916cd43dd8448eb211c80319c", "b9c7c989f97918e1", null, true, false, "congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=", "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01", "faithlifezipkin=00,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		[TestCase("4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", "b9c7c989f97918e1", true, true, "congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=", "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=03b9c7c989f97918e1,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		public void Inject(string traceId, string spanId, string parentSpanId, bool isSampled, bool isDebug, string context, string expectedTraceParent, string expectedTraceContext)
		{
			var traceIdHigh = long.Parse(traceId.Substring(0, 16), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var traceIdLow = long.Parse(traceId.Substring(16, 16), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var spanIdValue = long.Parse(spanId, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			var parentSpanIdValue = parentSpanId == null ? default(long?) : long.Parse(parentSpanId, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

			var w3cTraceContext = new W3CPropagation.W3CTraceContext(traceIdHigh, traceIdLow, spanIdValue, parentSpanIdValue, isSampled, isDebug, context);
			var (traceParent, traceContext) = W3CPropagation.CreateHeaderValues(w3cTraceContext);
			Assert.AreEqual(expectedTraceParent, traceParent);
			Assert.AreEqual(expectedTraceContext, traceContext);
		}

		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", "faithlifezipkin=00", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, false, false, null)]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=00", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, true, false, null)]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=01b9c7c989f97918e1", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", "b9c7c989f97918e1", true, false, null)]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=02", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, true, true, null)]
		[TestCase("00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01", "faithlifezipkin=00,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=", "0af7651916cd43dd8448eb211c80319c", "b9c7c989f97918e1", null, true, false, "congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "faithlifezipkin=03b9c7c989f97918e1,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", "b9c7c989f97918e1", true, true, "congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=,faithlifezipkin=03b9c7c989f97918e1", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", "b9c7c989f97918e1", true, true, "congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "rojo=00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", null, true, false, "rojo=00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		[TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "rojo=00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01,faithlifezipkin=03b9c7c989f97918e1,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=", "4bf92f3577b34da6a3ce929d0e0e4736", "00f067aa0ba902b7", "b9c7c989f97918e1", true, true, "rojo=00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01,congo=Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWF=")]
		public void Extract(string traceParent, string traceContext, string expectedTraceId, string expectedSpanId, string expectedParentSpanId, bool expectedIsSampled, bool expectedIsDebug, string expectedExtra)
		{
			var context = W3CPropagation.Extract(traceParent, traceContext);
			Assert.AreEqual(expectedTraceId, context.TraceIdHigh.ToString("x16") + context.TraceId.ToString("x16"));
			Assert.AreEqual(expectedSpanId, context.SpanId.ToString("x16"));
			Assert.AreEqual(expectedParentSpanId, context.ParentSpanId?.ToString("x16"));
			Assert.AreEqual(expectedIsSampled, context.Sampled ?? false);
			Assert.AreEqual(expectedIsDebug, context.Debug);
			Assert.AreEqual(expectedExtra, context.Extra?.First());
		}
	}
}
