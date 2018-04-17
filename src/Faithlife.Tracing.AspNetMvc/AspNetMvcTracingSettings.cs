using System;

namespace Faithlife.Tracing.AspNetMvc
{
	public sealed class AspNetMvcTracingSettings
	{
		public string ServiceName { get; set; }
		public Func<ITracer> CreateTracer { get; set; }
		public double? SamplingRate { get; set; }
	}
}
