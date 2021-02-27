using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace XunitSample
{
    public class DataDrivenTest
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void InlineDataTest(int num)
        {
            Assert.True(num > 0);
        }

        [Theory]
        [MemberData(nameof(TestMemberData))]
        public void MemberDataPropertyTest(int num)
        {
            Assert.True(num > 0);
        }

        public static IEnumerable<object[]> TestMemberData =>
            Enumerable.Range(1, 10)
                .Select(x => new object[] { x })
                .ToArray();

        [Theory]
        [MemberData(nameof(TestMemberDataField))]
        public void MemberDataFieldTest(int num)
        {
            Assert.True(num > 0);
        }

        public static readonly IList<object[]> TestMemberDataField = Enumerable.Range(1, 10).Select(x => new object[] { x }).ToArray();

        [Theory]
        [MemberData(nameof(TestMemberDataMethod), 10)]
        public void MemberDataMethodTest(int num)
        {
            Assert.True(num > 0);
        }

        public static IEnumerable<object[]> TestMemberDataMethod(int count)
        {
            return Enumerable.Range(1, count).Select(i => new object[] { i });
        }

        [Theory]
        [NullOrEmptyStringData]
        public void CustomDataAttributeTest(string value)
        {
            Assert.True(string.IsNullOrEmpty(value));
        }
    }

    public class NullOrEmptyStringDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
        }
    }
}
