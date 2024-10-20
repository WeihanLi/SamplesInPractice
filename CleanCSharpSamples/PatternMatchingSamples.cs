using System.Net;

namespace CleanCSharpSamples;

public static class PatternMatchingSamples
{
    public static void Run()
    {
        Console.WriteLine("Pattern Matching Samples");

        {
            object obj = new Cat("Mi");

            {
                if (obj == null)
                {
                }
                if (null == obj)
                {
                }
                if (obj is null)
                {
                }

                if (obj != null)
                {
                }
                if (obj is not null)
                {
                }
            }
            var cat0 = obj as Cat;
            if (cat0 != null)
            {
                Console.WriteLine($"Hello Cat: {cat0.Name}");
            }
            if (obj is Cat cat)
            {
                Console.WriteLine($"Hello Cat: {cat.Name}");
            }

            switch (obj)
            {
                case Cat c:
                    Console.WriteLine($"Hello Cat: {c.Name}");
                    break;
                
                case Dog d:
                    Console.WriteLine($"Hello Dog: {d.Name}");
                    break;
                
                case Mouse m:
                    Console.WriteLine($"Hello Mouse: {m.Name}");
                    break;
            }
        }
        
        // property pattern
        var a = new
        {
            FirstName = "Mike",
            Age = 10, 
            Job = new
            {
                Current = new
                {
                    Title = "Engineer",
                    Manager = new
                    {
                        FirstName = "John"
                    }
                },
                Histories = new[]
                {
                    new
                    {
                        Title = "Engineer I",
                    }
                }
            }
        };
        {
            if (a != null 
                && a.Age > 3 
                && a.Job != null 
                && a.Job.Current != null
                && a.Job.Histories != null
                && a.Job.Histories.Length > 0
                && a.Job.Current.Title == "Engineer" 
                && a.Job.Current.Manager != null
                && a.Job.Current.Manager.FirstName == "John"
                )
            {
                Console.WriteLine($"{a.FirstName} is {a.Age} years old {a.Job.Current.Title}");
            }
        }

        {
            if (a is 
                { 
                    Age : > 3, 
                    Job:
                    {
                        Histories.Length: > 0, 
                        Current:
                        {
                            Title: "Engineer", 
                            Manager.FirstName: "John"
                        }
                    }
                })
            {
                Console.WriteLine($"{a.FirstName} is {a.Age} years old {a.Job.Current.Title}");
            }
        }
    }
    
    public static void SwitchExpressionSample()
    {
        {
            var code = "Success";
            var returnCode = Enum.Parse<ReturnCode>(code, true);
            HttpStatusCode statusCode;
            switch (returnCode)
            {
                case ReturnCode.Success:
                    statusCode = HttpStatusCode.OK;
                    break;
                
                case ReturnCode.BadRequest:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                
                case ReturnCode.Unauthorized:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                
                case ReturnCode.Forbidden:
                    statusCode = HttpStatusCode.Forbidden;
                    break;
                
                case ReturnCode.InternalError:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
                
                default:
                    throw new ArgumentException("Invalid return code");
            }
            Console.WriteLine(statusCode);
        }
        
        {
            var code = "Success";
            var returnCode = Enum.Parse<ReturnCode>(code, true);
            var statusCode = returnCode switch
            {
                ReturnCode.Success => HttpStatusCode.OK,
                ReturnCode.BadRequest => HttpStatusCode.BadRequest,
                ReturnCode.Unauthorized => HttpStatusCode.Unauthorized,
                ReturnCode.Forbidden => HttpStatusCode.Forbidden,
                ReturnCode.InternalError => HttpStatusCode.InternalServerError,
                _ => throw new ArgumentException("Invalid return code")
            };
            Console.WriteLine(statusCode);
        }
    }

    public static void SwitchPatternSample()
    {
        {
            var text = "Hello World";
            var name = text switch
            {
                "Hello World" => "World",
                "Hello CSharp" or "Hello C#" => "C#",
                "Hello dotnet" => "dotnet",
                _ => "Unknown"
            };
            Console.WriteLine(name);
        }

        {
            // Relational patterns
            var score = int.Parse("66");
            var grade = score switch
            {
                < 30 => "D-",
                < 60 => "D",
                < 80 => "C",
                < 90 => "B",
                < 95 => "A",
                _ => "A+",
            };
            Console.WriteLine(grade);
        }
    }


    public static void ListPatternsSample()
    {
        {
            var array = new[]
            {
                1,2,3,4,5
            };

            if (array is [1, ..])
            {
                Console.WriteLine("The first one is 1");
            }
            if (array is [.., 5])
            {
                Console.WriteLine("The last one is 5");
            }
            if (array is [_, _, 3, ..])
            {
                Console.WriteLine("The third one is 3");
            }
            if (array is [.., 4, _])
            {
                Console.WriteLine("The second to last one is 4");
            }
            if (array is [1, 2, 3, ..])
            {
                Console.WriteLine("The sequence starts with 1,2,3");
            }
            if (array is [.., 3, 4, 5])
            {
                Console.WriteLine("The sequence ends with 3,4,5");
            }
            if (array is [.., 3, 4, _])
            {
                Console.WriteLine("The sequence ends with 3,4,_");
            }
        }
        {
            var objects = new object[]
            {
                1, "2", 3.0, "4", Guid.NewGuid()
            };

            if (objects is [1, "2", ..])
            {
                Console.WriteLine($"{objects[0]},{objects[1]}");
            }
            if (objects is [.., string str, Guid guid])
            {
                Console.WriteLine($"{str},{guid:N}");
            }

            var result = objects switch
            {
                [1, ..] => 1,
                [_, "2", ..] => 2,
                [_, _, double val, ..] => (int)val,
                [.., ""] => 4,
                [.., string strVal, Guid _] => Convert.ToInt32(strVal),
                _ => -1
            };
            Console.WriteLine(result);
        }
    }
    
    
    private static Point Transform(Point point) => point switch
    {
        { X: 0, Y: 0 }                    => new Point(0, 0),
        { X: var x, Y: var y } when x < y => new Point(x + y, y),
        { X: var x, Y: var y } when x > y => new Point(x - y, y),
        { X: var x, Y: var y }            => new Point(2 * x, 2 * y),
    };
}

// primary constructor
public readonly ref struct Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}


file enum ReturnCode
{
    Success = 0,
    BadRequest = 1,
    Unauthorized = 2,
    Forbidden = 3,
    InternalError = 99,
}

file abstract record Animal(string Name);
file sealed record Cat(string Name) : Animal(Name);
file sealed record Dog(string Name) : Animal(Name);
file sealed record Mouse(string Name) : Animal(Name);
