using WeihanLi.Common.Models;

namespace AspNetCoreWebDemo.Models
{
    public class User : BaseEntity
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
