using System;
using System.Collections.Generic;
using System.Linq;
using WeihanLi.Common.Models;

namespace CSharp9Sample
{
    internal class CleanCodeSample
    {
        // target-typed new expression
        //private static readonly Dictionary<string, Dictionary<string, string>>
        //    Dictionary = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, Dictionary<string, string>>
            Dictionary = new();

        public static void MainTest()
        {
            Console.WriteLine(nameof(Dictionary));
            PatternMatching();
            EnhancedPatternMatching();
            //SwitchExpression();

            //var result = NamedTuple();
            //Console.WriteLine(result.code);

            //IndexRange();
        }

        private static void TargetTypedNewExpression()
        {
            ReviewRequest[] requests =
            {
                new()
                {
                    State = ReviewState.Rejected
                },
                new(),
                new(),
            };
        }

        private static void PatternMatching()
        {
            object obj = "test";
            if (obj is string str)
            {
                Console.WriteLine(str);
            }
            SwitchPattern(obj);
            obj = 1;
            SwitchPattern(obj);

            void SwitchPattern(object obj0)
            {
                switch (obj0)
                {
                    case string str1:
                        Console.WriteLine(str1);
                        break;

                    case int num1:
                        Console.WriteLine(num1);
                        break;
                }
            }
        }

        private static void EnhancedPatternMatching()
        {
            IsInvalid('x');
            IsInvalidNew('x');

            static bool IsInvalid(char value)
            {
                var intValue = (int)value;
                if (intValue >= 48 && intValue <= 57)
                    return false;
                if (intValue >= 65 && intValue <= 90)
                    return false;
                if (intValue >= 97 && intValue <= 122)
                    return false;
                return intValue != 43 && intValue != 47;
            }

            static bool IsInvalidNew(char value)
            {
                var intValue = (int)value;
                return intValue switch
                {
                    >= 48 and <= 57 => false,
                    >= 65 and <= 90 => false,
                    >= 97 and <= 122 => false,
                    _ => intValue != 43 && intValue != 47
                };
            }
        }

        private static void SwitchExpression()
        {
            var state = ReviewState.Rejected;

            //var stateString = string.Empty;
            //switch (state)
            //{
            //    case ReviewState.Rejected:
            //        stateString = "0";
            //        break;

            //    case ReviewState.Reviewed:
            //        stateString = "1";
            //        break;

            //    case ReviewState.UnReviewed:
            //        stateString = "-1";
            //        break;
            //}

            var stateString = state switch
            {
                ReviewState.Rejected => "0",
                ReviewState.Reviewed => "1",
                ReviewState.UnReviewed => "-1",
                _ => string.Empty
            };

            Console.WriteLine($"{nameof(stateString)}:{stateString}");

            (int code, string msg) result = (0, "");
            var res = result switch
            {
                (0, _) => "success",
                (-1, _) => "xx",
                (-2, "") => "yy",
                (_, _) => "error"
            };
            Console.WriteLine(res);
        }

        private static (int code, string msg) NamedTuple()
        {
            return (0, string.Empty);
        }

        private static void IndexRange()
        {
            var arr = Enumerable.Range(1, 10).ToArray();
            Console.WriteLine($"last element:{arr[^1]}");

            var subArray = Enumerable.Range(1, 3).ToArray();
            Console.WriteLine(arr[..3].SequenceEqual(subArray) ? "StartWith" : "No");
        }
    }
}
