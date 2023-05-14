public class LambdaDefaultValueSample
{
    public static void MainTest()
    {
        var addWithDefault = (int addTo = 2) => addTo + 1;
        addWithDefault(); // 3
        addWithDefault(5); // 6
    }
}
