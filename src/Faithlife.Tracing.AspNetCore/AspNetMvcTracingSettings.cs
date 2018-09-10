using System;

namespace Faithlife.Tracing.AspNetCore
{
	public sealed class AspNetCoreTracingSettings
	{
		public string ServiceName { get; set; }
		public Func<ITracer> CreateTracer { get; set; }
		public double? SamplingRate { get; set; }
	}
}
