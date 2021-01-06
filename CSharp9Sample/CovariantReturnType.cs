namespace CSharp9Sample
{
    internal class CovariantReturnType
    {
        private abstract class Operation
        {
        }

        private abstract class OperationFactory
        {
            public abstract Operation GetOperation();
        }

        private class AddOperation : Operation
        {
        }

        private class AddOperationFactory : OperationFactory
        {
            public override AddOperation GetOperation()
            {
                return new();
            }
        }

        public static void MainTest()
        {
            var factory = new AddOperationFactory();
            factory.GetOperation();
        }
    }
}
