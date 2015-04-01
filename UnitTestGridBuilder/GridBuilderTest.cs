using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetGridBuilder;
using System.Web.UI.WebControls;

namespace UnitTestGridBuilder
{
    [TestClass]
    public class GridBuilderTest
    {
        private bool IsRowEqual<T>(Row<T> r1, Row<T> r2)
        {
            if ((r1.Children != null && r2.Children == null) || (r1.Children == null && r2.Children != null))
                return false;
            else if (r1.Children != null && r2.Children != null && r1.Children.Count != r2.Children.Count)
                return false;

            if ((r1.AggregateChildren != null && r2.AggregateChildren == null) || (r1.AggregateChildren == null && r2.AggregateChildren != null))
                return false;
            else if (r1.AggregateChildren != null && r2.AggregateChildren != null && r1.AggregateChildren.Count != r2.AggregateChildren.Count)
                return false;

            bool isEqual;
            if (r1.AggregateChildren != null)
            {
                for (int i = 0; i < r1.AggregateChildren.Count; i++)
                {
                    var aggChild1 = r1.AggregateChildren[i];
                    var aggChild2 = r2.AggregateChildren[i];

                    isEqual = aggChild1.Values.Equals(aggChild2.Values) 
                                   && aggChild1.AggregateName == aggChild2.AggregateName
                                   && ((aggChild1.Children == null && aggChild2.Children == null) || aggChild1.Children.Count == aggChild2.Children.Count)
                                   && aggChild1.ParentRow.AggregateChildren[i] == aggChild1
                                   && IsRowEqual(aggChild1, aggChild2);
                    if (!isEqual)
                        return false;
                }
                return true;
            }
            else if (r1.Children != null)
            {
                for (int i = 0; i < r1.Children.Count; i++)
                {
                    var child1 = r1.Children[i];
                    var child2 = r2.Children[i];

                    isEqual = child1.AggregateName == child2.AggregateName
                            && child1.Children == null && child2.Children == null
                            && child1.AggregateChildren == null && child2.AggregateChildren == null
                            && child1.Values.Equals(child2.Values)
                            && child1.ParentRow.Children[i] == child1;

                    if (!isEqual)
                        return false;
                }
                return true;
            }

            return r1.AggregateName == r2.AggregateName
                && r1.Values.Equals(r2.Values)
                && r1.ParentRow == null && r2.ParentRow == null;
        }

        [TestMethod]
        public void TestGridBuilder()
        {
            List<Student> students = new List<Student>()
            {
                new Student() { Name="Anna", Age=15, Gender="F", Grade=10, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="Rachel", Age=12, Gender="F", Grade=6, MathCourseMark=23, EnglishCourseMark=56, BiologyCourseMark=80, PhysicsCourseMark=34 },
                new Student() { Name="Ethan", Age=15, Gender="M", Grade=10, MathCourseMark=98, EnglishCourseMark=85, BiologyCourseMark=92, PhysicsCourseMark=97 },
                new Student() { Name="Sara", Age=16, Gender="F", Grade=10, MathCourseMark=28, EnglishCourseMark=73, BiologyCourseMark=69, PhysicsCourseMark=12 },
                new Student() { Name="Fred", Age=18, Gender="M", Grade=12, MathCourseMark=63, EnglishCourseMark=47, BiologyCourseMark=87, PhysicsCourseMark=56 },
                new Student() { Name="Brian", Age=15, Gender="M", Grade=10, MathCourseMark=54, EnglishCourseMark=38, PhysicsCourseMark=49 },
                new Student() { Name="Peter", Age=14, Gender="M", Grade=8, MathCourseMark=74, EnglishCourseMark=72, BiologyCourseMark=78, PhysicsCourseMark=65 },
                new Student() { Name="Lila", Age=18, Gender="F", Grade=12, MathCourseMark=92, EnglishCourseMark=93, PhysicsCourseMark=76 },
                new Student() { Name="Steve", Age=14, Gender="M", Grade=8, MathCourseMark=87, EnglishCourseMark=83, BiologyCourseMark=90, PhysicsCourseMark=68 },
                new Student() { Name="Sean", Age=16, Gender="M", Grade=10, MathCourseMark=74, EnglishCourseMark=93, BiologyCourseMark=86, PhysicsCourseMark=62 }                
            };

            SortParameter[] sortInfo = new SortParameter[] 
            {
                new SortParameter() { Attribute="Gender", Direction = SortDirection.Ascending},
                new SortParameter() { Attribute="Grade", Direction = SortDirection.Descending},
                new SortParameter() { Attribute="Age", Direction = SortDirection.Descending},
                new SortParameter() { Attribute="Name", Direction = SortDirection.Ascending }
            };

            string[] groupings = new string[] { "Gender", "Grade" };

            AggregationParameter[] aggregateParams = new AggregationParameter[]
            {
                new AggregationParameter() { Attribute="MathCourseMark", Type=AggregationType.Max },
                new AggregationParameter() { Attribute="EnglishCourseMark", Type=AggregationType.Min },
                new AggregationParameter() { Attribute="BiologyCourseMark", Type=AggregationType.Avg },
                new AggregationParameter() { Attribute="PhysicsCourseMark", Type=AggregationType.Sum }
            };

            List<Row<Student>> expected = new List<Row<Student>>();

            // female students
            Row<Student> femaleStudents = new Row<Student>();
            expected.Add(femaleStudents);
            femaleStudents.AggregateName = "F";
            femaleStudents.Values = new Student() { MathCourseMark = 92, EnglishCourseMark = 56, BiologyCourseMark = (211m / 4), PhysicsCourseMark = 198 };
            femaleStudents.AggregateChildren = new List<Row<Student>>();
            Row<Student> female12 = new Row<Student>();
            female12.AggregateName = "12";
            female12.ParentRow = femaleStudents;
            female12.Values = new Student() { Grade = 12, MathCourseMark = 92, EnglishCourseMark = 93, BiologyCourseMark = 0, PhysicsCourseMark = 76 };
            femaleStudents.AggregateChildren.Add(female12);
            female12.Children = new List<Row<Student>>()
            {
                new Row<Student> { ParentRow = female12, Values = students.Single(s => s.Name == "Lila") }
            };
            Row<Student> female10 = new Row<Student>();
            female10.AggregateName = "10";
            female10.ParentRow = femaleStudents;
            female10.Values = new Student() { Grade = 10, MathCourseMark = 80, EnglishCourseMark = 73, BiologyCourseMark = 65.5m, PhysicsCourseMark = 88 };
            femaleStudents.AggregateChildren.Add(female10);
            female10.Children = new List<Row<Student>>()
            {
                new Row<Student> { ParentRow = female10, Values = students.Single(s => s.Name == "Sara") },
                new Row<Student> { ParentRow = female10, Values = students.Single(s => s.Name == "Anna") }
            };
            Row<Student> female6 = new Row<Student>();
            female6.AggregateName = "6";
            female6.ParentRow = femaleStudents;
            female6.Values = new Student() { Grade = 6, MathCourseMark = 23, EnglishCourseMark = 56, BiologyCourseMark = 80, PhysicsCourseMark = 34 };
            femaleStudents.AggregateChildren.Add(female6);
            female6.Children = new List<Row<Student>>()
            {
                new Row<Student> { ParentRow = female6, Values = students.Single(s => s.Name == "Rachel") }
            };
            

            // male students
            Row<Student> maleStudents = new Row<Student>();
            expected.Add(maleStudents);
            maleStudents.AggregateName = "M";
            maleStudents.Values = new Student() { MathCourseMark = 92, EnglishCourseMark = 38, BiologyCourseMark = (433m / 6), PhysicsCourseMark = 397 };
            maleStudents.AggregateChildren = new List<Row<Student>>();
            Row<Student> male12 = new Row<Student>();
            male12.AggregateName = "12";
            male12.ParentRow = maleStudents;
            male12.Values = new Student() { Grade = 12, MathCourseMark = 63, EnglishCourseMark = 47, BiologyCourseMark = 87, PhysicsCourseMark = 56 };
            maleStudents.AggregateChildren.Add(male12);
            male12.Children = new List<Row<Student>>()
            {
                new Row<Student> { ParentRow = male12, Values = students.Single(s => s.Name == "Fred") }
            };
            Row<Student> male10 = new Row<Student>();
            male10.AggregateName = "10";
            male10.ParentRow = maleStudents;
            male10.Values = new Student() { Grade = 10, MathCourseMark = 98, EnglishCourseMark = 38, BiologyCourseMark = (178m / 3), PhysicsCourseMark = 208 };
            maleStudents.AggregateChildren.Add(male10);
            male10.Children = new List<Row<Student>>()
            {
                new Row<Student> { ParentRow = male10, Values = students.Single(s => s.Name == "Sean") },
                new Row<Student> { ParentRow = male10, Values = students.Single(s => s.Name == "Brian") },
                new Row<Student> { ParentRow = male10, Values = students.Single(s => s.Name == "Ethan") }
            };
            Row<Student> male8 = new Row<Student>();
            male8.AggregateName = "8";
            male8.ParentRow = maleStudents;
            male8.Values = new Student() { Grade = 8, MathCourseMark = 87, EnglishCourseMark = 72, BiologyCourseMark = 84, PhysicsCourseMark = 133 };
            maleStudents.AggregateChildren.Add(male8);
            male8.Children = new List<Row<Student>>()
            {
                new Row<Student> { ParentRow = male8, Values = students.Single(s => s.Name == "Peter") },
                new Row<Student> { ParentRow = male8, Values = students.Single(s => s.Name == "Steve") }
            };


            List<Row<Student>> actual = GridBuilder.BuildRows(students, sortInfo, groupings, aggregateParams);
            Assert.AreEqual(actual.Count, expected.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.IsTrue(IsRowEqual(actual[i], expected[i]));
            }
        }
    }

    internal class Student : Object
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public int Grade { get; set; }
        public decimal MathCourseMark { get; set; }
        public decimal EnglishCourseMark { get; set; }
        public decimal? BiologyCourseMark { get; set; }
        public decimal PhysicsCourseMark { get; set; }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Student other = (Student)obj;

            return this.Name == other.Name
                && this.Age == other.Age
                && this.Gender == other.Gender
                && this.Grade == other.Grade
                && this.MathCourseMark == other.MathCourseMark
                && this.EnglishCourseMark == other.EnglishCourseMark
                && this.BiologyCourseMark == other.BiologyCourseMark
                && this.PhysicsCourseMark == other.PhysicsCourseMark;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
