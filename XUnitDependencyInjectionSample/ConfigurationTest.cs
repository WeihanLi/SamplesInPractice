using Microsoft.Extensions.Configuration;
using Xunit;

namespace XUnitDependencyInjectionSample
{
    public class ConfigurationTest
    {
        private readonly IConfiguration _configuration;

        public ConfigurationTest(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Fact]
        public void ConfigurationGetTest()
        {
            var userName = _configuration["UserName"];
            Assert.Equal("Alice", userName);

            var enabled = _configuration.GetAppSetting<bool>("XxxEnabled");
            Assert.True(enabled);
        }
    }
}
