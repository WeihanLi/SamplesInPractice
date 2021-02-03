using System;

namespace UnexpectedSamples
{
    public class EnumParseSample
    {
        public static void MainTest()
        {
            Console.WriteLine(((Color)10).ToString());

            var intValue = int.MaxValue;
            Console.WriteLine(((Color)intValue).ToString());

            if (Enum.TryParse("10", out Color color))
            {
                Console.WriteLine(color.ToString());
            }
            if (Enum.TryParse("10", out Color color1)
                && Enum.IsDefined(typeof(Color), color1))
            {
                Console.WriteLine($"Success, {color1}");
            }
            else
            {
                Console.WriteLine("Can not match");
            }

            if (Enum.TryParse("Yellow", out Color color2))
            {
                Console.WriteLine(color2.ToString());
            }
            if (Enum.TryParse("Yellow", out Color color3)
                && Enum.IsDefined(typeof(Color), color3))
            {
                Console.WriteLine($"Success, {color3}");
            }
            else
            {
                Console.WriteLine("Can not match");
            }
        }
    }

    public enum Color : byte
    {
        Red = 0,
        Green = 1,
        Blue = 2,
    }
}
