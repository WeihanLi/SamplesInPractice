namespace Net9Samples;

public static class CSharp13Samples
{
    public static void ParamsCollectionSample()
    {
        ParamsArrayMethod(1, 2, 3);
        ParamsListMethod(1, 2, 3);
        ParamsEnumerableMethod(1, 2, 3);

        ParamsReadOnlySpanMethod(1, 2, 3);
        ParamsSpanMethod(1, 2, 3);

        void ParamsReadOnlySpanMethod(params Span<int> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsSpanMethod(params Span<int> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }
        void ParamsListMethod(params List<int> list)
        {
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsEnumerableMethod(params IEnumerable<int> array)
        {
            foreach (var item in array)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsArrayMethod(params int[] array)
        {
            foreach (var item in array)
            {
                Console.WriteLine(item);
            }
        }
    }


    //public class SemiFieldSample
    //{
    //    public string Name { get; set => field = value.Trim(); }
    //}
}
