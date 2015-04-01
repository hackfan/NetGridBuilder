using System;
using System.Collections.Generic;

namespace NetGridBuilder
{
    public class Row<T>
    {        
        public Row<T> ParentRow { get; set; }
        // if it's a leaf node, return null
        public List<Row<T>> AggregateChildren { get; set; }
        // only has value if there're no more aggregate children
        public List<Row<T>> Children { get; set; }
        // original object
        public T Values { get; set; }
        // grouping value
        public string AggregateName { get; set; }
    }
}
