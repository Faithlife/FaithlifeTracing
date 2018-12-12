using System.Globalization;
using NUnit.Framework;

namespace Faithlife.Tracing.Tests
{
	[TestFixture]
	public class NoopTracingTests
	{
		[Test]
		public void TraceSpanProvider_Properties_AreNotNull()
		{
			Assert.NotNull(NoopTraceSpanProvider.Instance.CurrentSpan);
		}

		[Test]
		public void TraceSpan_Properties_AreNotNull()
		{
			Assert.NotNull(NoopTraceSpan.Instance.CurrentSpan);
			Assert.NotNull(NoopTraceSpan.Instance.SpanId);
			Assert.NotNull(NoopTraceSpan.Instance.TraceId);
			Assert.NotNull(NoopTraceSpan.Instance.Tracer);
		}

		[Test]
		public void TraceSpan_StartChildSpan_NotNull()
		{
			Assert.NotNull(NoopTraceSpan.Instance.StartChildSpan(TraceSpanKind.Client, new[] { (SpanTagNames.Operation, "test") }));
		}

		[Test]
		public void Tracer_StartSpan_NotNull()
		{
			Assert.NotNull(NoopTracer.Instance.StartSpan(NoopTraceSpan.Instance, TraceSpanKind.Client, new[] { (SpanTagNames.Operation, "test") }));
		}

		[Test]
		public void Tracer_ExtractSpan_NotNull()
		{
			Assert.NotNull(NoopTracer.Instance.ExtractSpan(_ => ""));
		}
	}
}
