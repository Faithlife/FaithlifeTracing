using System;

namespace Faithlife.Tracing.AspNetWebApi
{
	public sealed class AspNetWebApiTracingSettings
	{
		public string ServiceName { get; set; }
		public Func<ITracer> CreateTracer { get; set; }
		public double? SamplingRate { get; set; }
	}
}
