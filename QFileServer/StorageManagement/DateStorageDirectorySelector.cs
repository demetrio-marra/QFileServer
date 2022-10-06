using QFileServer.Configuration;

namespace QFileServer.StorageManagement
{
    public class DateStorageDirectorySelector : IStorageDirectorySelector
    {
        readonly QFileServerServiceConfiguration configuration;
        readonly ILogger<DateStorageDirectorySelector> logger;

        public DateStorageDirectorySelector(QFileServerServiceConfiguration configuration,
            ILogger<DateStorageDirectorySelector> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        async Task<string> IStorageDirectorySelector.GetStorageDirectory(string fileName)
        {
            var targetDir = Path.Combine(configuration.FileServerRootPath, DateTime.Now.Date.ToString("yyyyMMdd"));
            logger.LogDebug("GetStorageDirectory returning {targetDir} for fileName {fileName}",
                targetDir, fileName);
            return await Task.FromResult(targetDir);
        }
    }
}
