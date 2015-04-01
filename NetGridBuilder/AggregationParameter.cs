using System;

namespace NetGridBuilder
{
    public enum AggregationType
    {
        Sum,
        Min,
        Max,
        Avg
    }

    public class AggregationParameter
    {
        public string Attribute { get; set; }
        public AggregationType Type { get; set; }
    }
}
