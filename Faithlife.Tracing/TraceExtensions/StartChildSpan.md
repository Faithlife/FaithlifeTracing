# TraceExtensions.StartChildSpan method

Starts a new [`ITraceSpan`](../ITraceSpan.md) that is a child of *traceSpan*.

```csharp
public static ITraceSpan StartChildSpan(this ITraceSpan traceSpan, TraceSpanKind kind, 
    IEnumerable<ValueTuple<string, string>> tags)
```

| parameter | description |
| --- | --- |
| traceSpan | (Optional) The current span. If this is `null`, this method returns `null`. |
| kind | The [`TraceSpanKind`](../TraceSpanKind.md). |
| tags | A sequence of span tags; see [`SetTag`](../ITraceSpan/SetTag.md). |

## Return Value

A new [`ITraceSpan`](../ITraceSpan.md), or `null`.

## See Also

* interface [ITraceSpan](../ITraceSpan.md)
* enum [TraceSpanKind](../TraceSpanKind.md)
* class [TraceExtensions](../TraceExtensions.md)
* namespace [Faithlife.Tracing](../../Faithlife.Tracing.md)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.Tracing.dll -->