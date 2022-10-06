namespace QFileServer.StorageManagement
{
    public interface IStorageDirectorySelector
    {
        Task<string> GetStorageDirectory(string fullFilePath);
    }
}
