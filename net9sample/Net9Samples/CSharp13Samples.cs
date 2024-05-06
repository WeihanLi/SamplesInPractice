namespace Net9Samples;

public static class CSharp13Samples
{
    public static void ParamsCollectionSample()
    {
        ParamsMethod(1, 2, 3);

        void ParamsMethod(params Span<int> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }
    }
}
