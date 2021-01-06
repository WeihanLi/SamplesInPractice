using System;
using System.Linq.Expressions;

namespace CSharp9Sample
{
    internal class StaticAnonymousMethod
    {
        private readonly int num = 1;

        public void MainTest()
        {
            // anonymous method
            Action action = () => { Console.WriteLine(num); };
            Action action1 = static () => { };

            //expression
            Expression<Func<int, bool>> expression = i => i > num;
            Expression<Func<int, bool>> expression1 = static i => i > 1;
        }
    }
}
