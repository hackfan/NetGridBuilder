using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetGridBuilder;

namespace UnitTestGridBuilder
{
    [TestClass]
    public class GridBuilderTest
    {
        private bool IsRowEqual<T>(Row<T> r1, Row<T> r2)
        {
            if ((r1.Children != null && r2.Children == null) || (r1.Children == null && r2.Children != null))
                return false;
            else if (r1.Children.Count != r2.Children.Count)
                return false;

            if ((r1.AggregateChildren != null && r2.AggregateChildren == null) || (r1.AggregateChildren == null && r2.AggregateChildren != null))
                return false;
            else if (r1.AggregateChildren.Count != r2.AggregateChildren.Count)
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
                                   && aggChild1.Children == null && aggChild2.Children == null
                                   && aggChild1.ParentRow.AggregateChildren[i] == aggChild1
                                   && IsRowEqual(aggChild1, aggChild2);
                    if (!isEqual)
                        return false;
                }
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
                new Student() { Name="A", Age=16, Gender="F", Grade=10, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="V", Age=12, Gender="F", Grade=6, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="E", Age=10, Gender="M", Grade=4, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="B", Age=16, Gender="F", Grade=10, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="F", Age=18, Gender="M", Grade=12, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="L", Age=15, Gender="M", Grade=10, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="P", Age=13, Gender="M", Grade=8, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="C", Age=18, Gender="F", Grade=12, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="R", Age=14, Gender="M", Grade=8, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 },
                new Student() { Name="O", Age=16, Gender="M", Grade=10, MathCourseMark=80, EnglishCourseMark=83, BiologyCourseMark=62, PhysicsCourseMark=76 }                
            };
        }
    }

    internal class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public int Grade { get; set; }
        public decimal MathCourseMark { get; set; }
        public decimal EnglishCourseMark { get; set; }
        public decimal? BiologyCourseMark { get; set; }
        public decimal PhysicsCourseMark { get; set; }

        public bool Equals(Student other)
        {
            return this.Name == other.Name
                && this.Age == other.Age
                && this.Gender == other.Gender
                && this.Year == other.Year
                && this.MathCourseMark == other.MathCourseMark
                && this.EnglishCourseMark == other.EnglishCourseMark
                && this.BiologyCourseMark == other.BiologyCourseMark
                && this.PhysicsCourseMark == other.PhysicsCourseMark;
        }
    }
}
