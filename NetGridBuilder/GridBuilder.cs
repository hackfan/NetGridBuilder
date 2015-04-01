using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Web;
using System.Reflection;
using System.Web.UI.WebControls;

namespace NetGridBuilder
{
    public class GridBuilder
    {
        /// <summary>
        /// Sort, group and aggregate any collection of objects into a list of Rows
        /// </summary>
        /// <typeparam name="T">generic object</typeparam>
        /// <param name="records">collection of objects</param>
        /// <param name="orderParams">array of sort parameters (property name and sort direction)</param>
        /// <param name="groupParams">array of property names</param>
        /// <param name="aggregateParams">array of aggregation parameters (property name and aggregation type)</param>
        /// <returns></returns>
        public static List<Row<T>> BuildRows<T>(IEnumerable<T> records, SortParameter[] orderParams, string[] groupParams, AggregationParameter[] aggregateParams)
        {
            List<Row<T>> rows = new List<Row<T>>();

            if (records != null && records.Count() > 0)
            {
                // validate properties' names
                var properties = records.ElementAt(0).GetType().GetProperties().Select(p => p.Name);
                string[] checkProps = groupParams;
                if (orderParams != null && orderParams.Length > 0)
                {
                    if (checkProps != null)
                        checkProps = orderParams.Select(op => op.Attribute).Union(checkProps).ToArray();
                    else
                        checkProps = orderParams.Select(op => op.Attribute).ToArray();
                }
                if (aggregateParams != null && aggregateParams.Length > 0)
                {
                    if (checkProps != null)
                        checkProps = aggregateParams.Select(op => op.Attribute).Union(checkProps).ToArray();
                    else
                        checkProps = aggregateParams.Select(op => op.Attribute).ToArray();
                }
                if (checkProps != null)
                {
                    foreach (var prop in checkProps)
                    {
                        if (!properties.Contains(prop))
                            throw new Exception("Invalid property name: " + prop);
                    }
                }

                // Sort records
                var sortedRecords = SortRecords(records.AsQueryable(), orderParams).ToList();

                // Group records
                rows = GroupRecords(sortedRecords, groupParams);

                // Aggregate values for groupings
                foreach (var row in rows)
                    AggregateRowValue(row, aggregateParams);
            }

            return rows;
        }

        public static List<Row<T>> GroupRecords<T>(IEnumerable<T> records, string[] groupParams)
        {
            List<Row<T>> rows = new List<Row<T>>();
            if (records != null)
            {
                if (groupParams != null && groupParams.Length > 0)
                {
                    foreach (var rec in records)
                    {
                        var propValue = rec.GetType().GetProperty(groupParams[0]).GetValue(rec, null);
                        var rootRow = rows.AsQueryable().Where(string.Format("ParentRow == null && Values.{0} == @0", groupParams[0]), propValue).SingleOrDefault();
                        // if grouping row doesn't exist, create a new one and insert it into the result list
                        if (rootRow == null)
                        {
                            rootRow = new Row<T>();
                            rootRow.AggregateName = propValue.ToString();
                            rootRow.Values = (T)Activator.CreateInstance(rec.GetType());
                            rootRow.Values.GetType().GetProperty(groupParams[0]).SetValue(rootRow.Values, propValue, null);
                            rows.Add(rootRow);
                        }
                        Row<T> currRow = new Row<T>();
                        currRow.Values = rec;
                        InsertChildRow<T>(rootRow, currRow, groupParams, 1);
                    }
                }
                else
                {
                    // if there's no grouping, create a new row for every record
                    foreach (var rec in records)
                    {
                        Row<T> currRow = new Row<T>();
                        currRow.Values = rec;
                        rows.Add(currRow);
                    }
                }
            }
            return rows;
        }

        private static void InsertChildRow<T>(Row<T> parentRow, Row<T> childRow, string[] groupParams, int index)
        {
            if (parentRow == null || childRow == null || groupParams == null)
                throw new Exception("parentRow, childRow and groupParams cannot be null");

            if (index > groupParams.Length)
                throw new Exception("Index is > than number of groupings. Something went wrong.");

            if (index < groupParams.Length)
            {
                Row<T> aggRow = null;
                var propValue = childRow.Values.GetType().GetProperty(groupParams[index]).GetValue(childRow.Values, null);
                if (parentRow.AggregateChildren != null)
                {
                    aggRow = parentRow.AggregateChildren.AsQueryable().Where(string.Format("Values.{0} == @0", groupParams[index]), propValue).SingleOrDefault();
                }
                else
                {
                    parentRow.AggregateChildren = new List<Row<T>>();
                }

                if (aggRow == null)
                {
                    aggRow = new Row<T>();
                    aggRow.AggregateName = propValue.ToString();
                    aggRow.Values = (T)Activator.CreateInstance(childRow.Values.GetType());
                    aggRow.Values.GetType().GetProperty(groupParams[index]).SetValue(aggRow.Values, propValue, null);
                    aggRow.ParentRow = parentRow;
                    parentRow.AggregateChildren.Add(aggRow);
                }

                InsertChildRow<T>(aggRow, childRow, groupParams, ++index);
            }
            else
            {
                // there's no more grouping, insert the record to children
                if (parentRow.Children == null)
                    parentRow.Children = new List<Row<T>>();

                parentRow.Children.Add(childRow);
                childRow.ParentRow = parentRow;
            }
        }

        public static List<T> SortRecords<T>(IEnumerable<T> records, SortParameter[] orderParams)
        {
            if (records == null)
                throw new Exception("records cannot be null");

            if (orderParams == null || orderParams.Length == 0)
                return records.ToList();

            var query = records.AsQueryable();
            SortParameter orderBy = orderParams[0];
            string orderMethodName = orderBy.Direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            Type t = typeof(T);
            var param = Expression.Parameter(t, "rec");
            var property = t.GetProperty(orderBy.Attribute);

            // Create an express tree that represents the expression OrderBy(rec => rec.<T>) or OrderByDescending(rec => rec.<T>)
            MethodCallExpression orderByExp = Expression.Call(
                                                    typeof(Queryable),
                                                    orderMethodName,
                                                    new Type[] { t, property.PropertyType },
                                                    query.Expression,
                                                    Expression.Quote(
                                                        Expression.Lambda(
                                                            Expression.Property(param, property),
                                                            param))
                                               );

            for (int i = 1; i < orderParams.Length; i++)
            {
                orderBy = orderParams[i];
                orderMethodName = orderBy.Direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending";
                property = t.GetProperty(orderBy.Attribute);

                // Append ThenBy/ThenByDescending expression
                orderByExp = Expression.Call(
                                    typeof(Queryable),
                                    orderMethodName,
                                    new Type[] { t, property.PropertyType },
                                    orderByExp,
                                    Expression.Quote(
                                        Expression.Lambda(
                                            Expression.Property(param, property),
                                            param))
                                );
            }

            return query.Provider.CreateQuery<T>(orderByExp).ToList();
        }

        private static void AggregateRowValue<T>(Row<T> aggregateRow, AggregationParameter[] aggregateParams)
        {
            if (aggregateRow == null)
                throw new Exception("aggregateRow cannot be null");

            if (aggregateParams != null && aggregateParams.Length > 0)
            {
                foreach (var aggParam in aggregateParams)
                {
                    string propertyName = aggParam.Attribute;
                    switch (aggParam.Type)
                    {
                        case AggregationType.Avg:
                            SetAverageValue(aggregateRow, propertyName);
                            break;
                        case AggregationType.Sum:
                            SetSumValue(aggregateRow, propertyName);
                            break;
                        case AggregationType.Min:
                            SetMinValue(aggregateRow, propertyName);
                            break;
                        case AggregationType.Max:
                            SetMaxValue(aggregateRow, propertyName);
                            break;
                    }
                }
            }
        }

        private static void SetSumValue<T>(Row<T> row, string propertyName)
        {
            PropertyInfo rowProp = row.Values.GetType().GetProperty(propertyName);
            dynamic defaultVal = Activator.CreateInstance(Nullable.GetUnderlyingType(rowProp.PropertyType) ?? rowProp.PropertyType);
            if (!IsNumericType(rowProp.PropertyType))
                return;

            if (row.AggregateChildren != null && row.AggregateChildren.Count > 0)
            {
                dynamic aggSum = Activator.CreateInstance(rowProp.PropertyType) ?? defaultVal;
                foreach (var childRow in row.AggregateChildren)
                {
                    SetSumValue(childRow, propertyName);
                    dynamic aggRowVal = childRow.Values.GetType().GetProperty(propertyName).GetValue(childRow.Values, null) ?? defaultVal;
                    aggSum += aggRowVal;
                }
                rowProp.SetValue(row.Values, aggSum, null);
            }
            else
            {
                if (row.Children != null && row.Children.Count > 0)
                {
                    dynamic sum = row.Children[0].Values.GetType().GetProperty(propertyName).GetValue(row.Children[0].Values, null) ?? defaultVal;
                    for (int i = 1; i < row.Children.Count; i++)
                    {
                        dynamic rowVal = row.Children[i].Values.GetType().GetProperty(propertyName).GetValue(row.Children[i].Values, null) ?? defaultVal;
                        sum += rowVal;
                    }

                    rowProp.SetValue(row.Values, sum, null);
                }
            }
        }

        private static int SetAverageValue<T>(Row<T> row, string propertyName)
        {
            PropertyInfo rowProp = row.Values.GetType().GetProperty(propertyName);
            dynamic defaultVal = Activator.CreateInstance(Nullable.GetUnderlyingType(rowProp.PropertyType) ?? rowProp.PropertyType);
            if (!IsNumericType(rowProp.PropertyType))
                return 0;

            if (row.AggregateChildren != null && row.AggregateChildren.Count > 0)
            {
                dynamic totalChildren = 0;
                dynamic aggSum = Activator.CreateInstance(rowProp.PropertyType) ?? defaultVal;
                foreach (var childRow in row.AggregateChildren)
                {
                    dynamic rowTotalChildren = SetAverageValue(childRow, propertyName);
                    totalChildren += rowTotalChildren;
                    dynamic aggRowSum = childRow.Values.GetType().GetProperty(propertyName).GetValue(childRow.Values, null) ?? defaultVal;
                    dynamic aggRowVal = aggRowSum * rowTotalChildren;
                    aggSum += aggRowVal;
                }
                rowProp.SetValue(row.Values, aggSum / totalChildren, null);
                return totalChildren;
            }
            else
            {
                if (row.Children != null && row.Children.Count > 0)
                {
                    dynamic sum = row.Children[0].Values.GetType().GetProperty(propertyName).GetValue(row.Children[0].Values, null) ?? defaultVal;
                    for (int i = 1; i < row.Children.Count; i++)
                    {
                        dynamic rowVal = row.Children[i].Values.GetType().GetProperty(propertyName).GetValue(row.Children[i].Values, null) ?? defaultVal;
                        sum += rowVal;
                    }

                    rowProp.SetValue(row.Values, sum / row.Children.Count, null);
                    return row.Children.Count;
                }
            }

            return 0;
        }

        private static void SetMinValue<T>(Row<T> row, string propertyName)
        {
            PropertyInfo rowProp = row.Values.GetType().GetProperty(propertyName);
            if (!IsNumericType(rowProp.PropertyType) && rowProp.PropertyType != typeof(DateTime))
                return;

            if (row.AggregateChildren != null && row.AggregateChildren.Count > 0)
            {
                dynamic aggMin = Activator.CreateInstance(rowProp.PropertyType);
                for (int i = 0; i < row.AggregateChildren.Count; i++)
                {
                    var childRow = row.AggregateChildren[i];

                    SetMinValue(childRow, propertyName);
                    dynamic aggVal = childRow.Values.GetType().GetProperty(propertyName).GetValue(childRow.Values, null);
                    if (i == 0)
                        aggMin = aggVal;

                    if (aggVal != null && aggMin == null)
                        aggMin = aggVal;
                    else if (aggVal != null && aggMin != null && aggVal < aggMin)
                        aggMin = aggVal;
                }
                rowProp.SetValue(row.Values, aggMin, null);
            }
            else
            {
                if (row.Children != null && row.Children.Count > 0)
                {
                    dynamic min = row.Children[0].Values.GetType().GetProperty(propertyName).GetValue(row.Children[0].Values, null);
                    for (int i = 1; i < row.Children.Count; i++)
                    {
                        dynamic val = row.Children[i].Values.GetType().GetProperty(propertyName).GetValue(row.Children[i].Values, null);

                        if (val != null && min == null)
                            min = val;
                        else if (val != null && min != null && val < min)
                            min = val;
                    }

                    rowProp.SetValue(row.Values, min, null);
                }
            }
        }

        private static void SetMaxValue<T>(Row<T> row, string propertyName)
        {
            PropertyInfo rowProp = row.Values.GetType().GetProperty(propertyName);
            if (!IsNumericType(rowProp.PropertyType) && rowProp.PropertyType != typeof(DateTime))
                return;

            if (row.AggregateChildren != null && row.AggregateChildren.Count > 0)
            {
                dynamic aggMax = Activator.CreateInstance(rowProp.PropertyType);
                for (int i = 0; i < row.AggregateChildren.Count; i++)
                {
                    var childRow = row.AggregateChildren[i];

                    SetMaxValue(childRow, propertyName);
                    dynamic aggVal = childRow.Values.GetType().GetProperty(propertyName).GetValue(childRow.Values, null);
                    if (i == 0)
                        aggMax = aggVal;

                    if (aggVal != null && aggMax == null)
                        aggMax = aggVal;
                    else if (aggVal != null && aggMax != null && aggVal > aggMax)
                        aggMax = aggVal;
                }
                rowProp.SetValue(row.Values, aggMax, null);
            }
            else
            {
                if (row.Children != null && row.Children.Count > 0)
                {
                    dynamic max = row.Children[0].Values.GetType().GetProperty(propertyName).GetValue(row.Children[0].Values, null);
                    for (int i = 1; i < row.Children.Count; i++)
                    {
                        dynamic val = row.Children[i].Values.GetType().GetProperty(propertyName).GetValue(row.Children[i].Values, null);

                        if (val != null && max == null)
                            max = val;
                        else if (val != null && max != null && val > max)
                            max = val;
                    }

                    rowProp.SetValue(row.Values, max, null);
                }
            }
        }

        private static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }
    }
}