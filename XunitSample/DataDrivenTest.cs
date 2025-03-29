using System.Collections.Generic;
using System.Linq;
using Xunit;

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

        public static IEnumerable<TheoryDataRow<int>> TestMemberData =>
            Enumerable.Range(1, 10)
                .Select(i => new TheoryDataRow<int>(i))
            ;

        [Theory]
        [MemberData(nameof(TestMemberDataField))]
        public void MemberDataFieldTest(int num)
        {
            Assert.True(num > 0);
        }

        public static readonly IList<TheoryDataRow<int>> TestMemberDataField = 
            Enumerable.Range(1, 10).Select(x => new TheoryDataRow<int>(x)).ToArray();

        [Theory]
        [MemberData(nameof(TestMemberDataMethod), 10)]
        public void MemberDataMethodTest(int num)
        {
            Assert.True(num > 0);
        }

        public static IEnumerable<TheoryDataRow<int>> TestMemberDataMethod(int count)
        {
            return Enumerable.Range(1, count).Select(i => new TheoryDataRow<int>(i));
        }

        [Theory]
        [ClassData(typeof(NullOrEmptyStringDataAttribute))]
        public void CustomDataAttributeTest(string value)
        {
            Assert.True(string.IsNullOrEmpty(value));
        }
    }

    public class NullOrEmptyStringDataAttribute : TheoryData<string>
    {
        public NullOrEmptyStringDataAttribute()
        {
            Add((string)null);
            Add(string.Empty);
        }
    }
}
