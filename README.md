# NetGridBuilder
A library to dynamically sort, group and aggregate any properties for a collection of any objects that you pass in

## Supported Version
.Net 4.5 

## Features
*	**Sorting**: Ascending/Descending
*	**Grouping**: by property name
*	**Aggregation** on property (nullable accepted): 
  *	**Sum**: only supports numeric values
  *	**Avg**: only supports numeric values
  *	**Min**: only supports numeric values and DateTime
  *	**Max**: only supports numeric values and DateTime

## Sample Code
```csharp
SortParameter[] sortInfo = new SortParameter[] 
{
   new SortParameter() { Attribute="Gender", Direction = SortDirection.Ascending},
   new SortParameter() { Attribute="Age", Direction = SortDirection.Descending},
   new SortParameter() { Attribute="Name", Direction = SortDirection.Ascending }
};
string[] groupings = new string[] { "Gender", "Age" };
AggregationParameter[] aggregateParams = new AggregationParameter[]
{
   new AggregationParameter() { Attribute="MathCourseMark", Type=AggregationType.Max },
   new AggregationParameter() { Attribute="EnglishCourseMark", Type=AggregationType.Min },
   new AggregationParameter() { Attribute="BiologyCourseMark", Type=AggregationType.Avg },
   new AggregationParameter() { Attribute="PhysicsCourseMark", Type=AggregationType.Sum }
};
List<Row<Student>> studentRows = GridBuilder.BuildRows(students, sortInfo, groupings, aggregateParams);
```

## License
(The MIT License)

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
'Software'), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
