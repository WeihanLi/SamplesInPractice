using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace ScriptEngine.Test
{
    public class ConditionEvalTest
    {
        [Fact]
        public async Task EvalTest()
        {
            var condition = "x+y > 10";
            var variables = JsonConvert.SerializeObject(new[]
            {
                new
                {
                    Name = "x",
                    Type = "int"
                },
                new
                {
                    Name = "y",
                    Type = "int"
                },
            });

            var params1 = new
            {
                x = 1,
                y = 3
            };
            Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1));

            var params2 = new
            {
                x = 6,
                y = 5
            };
            Assert.True(await ScriptEngine.EvalAsync(condition, variables, params2));
        }

        [Fact]
        public async Task EvalStringTest()
        {
            var condition = "x > y.Length";
            var variables = JsonConvert.SerializeObject(new[]
            {
                new
                {
                    Name = "x",
                    Type = "int"
                },
                new
                {
                    Name = "y",
                    Type = "string"
                },
            });

            var params1 = new
            {
                x = 1,
                y = "3"
            };
            Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1));

            var params2 = new
            {
                x = 6,
                y = "5211"
            };
            Assert.True(await ScriptEngine.EvalAsync(condition, variables, params2));
        }

        [Fact]
        public async Task EvalLinqTest()
        {
            var condition = "list.Any(x=>x>10)";
            var variables = JsonConvert.SerializeObject(new[]
            {
                new
                {
                    Name = "list",
                    Type = "List<int>"
                }
            });

            var params1 = new
            {
                list = new List<int>()
                {
                    1,2,3,4,5
                }
            };
            Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1));

            var params2 = new
            {
                list = new List<int>()
                {
                    1,2,3,4,5,10,12
                }
            };
            Assert.True(await ScriptEngine.EvalAsync(condition, variables, params2));
        }
    }
}
