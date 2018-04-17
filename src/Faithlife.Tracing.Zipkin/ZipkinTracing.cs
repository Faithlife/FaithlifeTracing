using System;

namespace Faithlife.Tracing.Zipkin
{
	public static class ZipkinTracing
	{
		public static ITracer CreateTracer(Uri collectorUri) => new ZipkinTracer(collectorUri);
	}
}
