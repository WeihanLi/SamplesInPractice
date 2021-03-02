using System;
using System.Collections.Generic;
using WeihanLi.Extensions;

namespace CSharp9Sample
{
    public static class IndexRangeSample
    {
        public static void MainTest()
        {
            var someArray = new int[] { 1, 2, 3, 4, 5 };
            someArray.Dump();

            var ab = someArray[..^2];

            var list = new List<int>(someArray[1..^1]);
            someArray.Dump();

            var lastElement = someArray[^1]; // lastElement = 5
            lastElement.Dump();

            someArray[3..5].Dump();

            someArray[1..^1].Dump();
            someArray[1..].Dump();

            someArray[..^1].Dump();

            someArray[..2].Dump();

            var array = new TestCollection()
            {
                Data = new[] { 1, 2, 3 }
            };
            Console.WriteLine(array[^1]);
            array[1..2].Dump();
        }

        private class TestCollection
        {
            public IList<int> Data { get; init; }
            public int Count => Data.Count;
            public int this[int index] => Data[index];

            //public int this[Index index] => Data[index.GetOffset(Data.Count)];

            //public int[] this[Range range]
            //{
            //    get
            //    {
            //        var rangeInfo = range.GetOffsetAndLength(Data.Count);
            //        return Data.Skip(rangeInfo.Offset).Take(rangeInfo.Length).ToArray();
            //    }
            //}

            public int[] Slice(int start, int length)
            {
                var array = new int[length];
                for (var i = start; i < length && i < Data.Count; i++)
                {
                    array[i] = Data[i];
                }
                return array;
            }
        }
    }
}
