using System.ComponentModel;
using System.Linq.Expressions;

namespace CSharp10Sample;

public class LamdbaEnhancement
{
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/lambda-improvements?WT.mc_id=DT-MVP-5004222
    // https://devblogs.microsoft.com/dotnet/welcome-to-csharp-10/?WT.mc_id=DT-MVP-5004222#improvements-for-lambda-expressions-and-method-groups
    public static void MainTest()
    {
        // Natural types for lambdas
        // Func<int> func = () => 1;
        var func = () => 1;
        // Func<string> func2 = ()=>"Hello";
        var func2 = () => "Hello";

        // Func<string, int> parse = (string s) => int.Parse(s);
        var parse = (string s) => int.Parse(s);

        // ref/out/in
        var refFunc = (ref int x) => { x++; };
        var outFunc = (out int x) => { x = -1; };
        var inFunc = (in int x) => { };

        var num = 1;
        refFunc(ref num);
        Console.WriteLine(num);

        outFunc(out num);
        Console.WriteLine(num);

        // return type
        var lambdaWithReturnValue0 = int? () => null;
        // return type and input type
        var lambdaWithReturnValue1 = int? (string s)
            => string.IsNullOrEmpty(s) ? 1 : null;
        // Func<bool, object>
        var choose = object (bool b) => b ? 1 : "two";

        // Lambda method
        // Action<string> func3 = LocalMethod;
        var func3 = LocalMethod;
        void LocalMethod(string a)
        {
            Console.WriteLine(a);
        }

        var checkFunc = string.IsNullOrEmpty;
        var read = Console.Read;
        Action<string> write = Console.Write;

        // Attribute
        var parse3 =[Description("Lambda attribute")](string s) => int.Parse(s);
        var choose3 =[Description("Lambda attribute")]object (bool b) => b ? 1 : "two";

        // Expression
        Expression<Func<string, int>> expr = (string s) => int.Parse(s);
        LambdaExpression parseExpr = object (bool b) => b ? 1 : "two";
        Expression parseExpr1 = int? () => null;
    }

    public static Func<int?> ReturnValue(bool flag)
    {
        return flag ? () => null : () => 1;
    }
}
