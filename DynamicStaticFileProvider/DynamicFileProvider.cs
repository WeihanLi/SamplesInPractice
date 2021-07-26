using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace DynamicStaticFileProvider
{
    public class DynamicFileProviderOptions
    {
        public string CurrentSlot { get; set; }
    }

    public class DynamicFileProvider : IFileProvider
    {
        private const string DefaultSlotName = "Slot1";
        private PhysicalFileProvider _physicalFileProvider;

        public DynamicFileProvider(IOptionsMonitor<DynamicFileProviderOptions> optionsMonitor,
            IWebHostEnvironment webHostEnvironment)
        {
            var webRoot = webHostEnvironment.ContentRootPath;
            _physicalFileProvider =
                new PhysicalFileProvider(Path.Combine(webRoot,
                    optionsMonitor.CurrentValue.CurrentSlot ?? DefaultSlotName));
            optionsMonitor.OnChange(options =>
            {
                var path = Path.Combine(webRoot, options.CurrentSlot);
                _physicalFileProvider = new PhysicalFileProvider(path);
            });
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _physicalFileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _physicalFileProvider.GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _physicalFileProvider.Watch(filter);
        }
    }
}
