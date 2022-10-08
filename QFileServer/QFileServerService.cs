using AutoMapper;
using QFileServer.Data;
using QFileServer.Data.Entities;
using QFileServer.Models;
using QFileServer.StorageManagement;

namespace QFileServer
{
    public class QFileServerService
    {
        readonly QFileServerRepository repository;
        readonly IMapper mapper;
        readonly IStorageDirectorySelector storageDirectorySelector;
        readonly ILogger<QFileServerService> logger;

        public QFileServerService(QFileServerRepository repository,
            IMapper mapper,
            IStorageDirectorySelector storageDirectorySelector,
            ILogger<QFileServerService> logger)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.storageDirectorySelector = storageDirectorySelector;
            this.logger = logger;
        }

        public IQueryable<QFileServerModel> GetAllFilesOData()
        {
            var entities = repository.GetAllOData();
            return mapper.ProjectTo<QFileServerModel>(entities);
        }

        public async Task<IEnumerable<QFileServerModel>> GetAllFiles()
        {
            var models = mapper.Map<IEnumerable<QFileServerModel>>(await repository.GetAll());
            logger.LogInformation("GetAllFiles returned {filesCount} elements", models.Count());
            return models;
        }

        public async Task<QFileServerModel?> GetFile(long id)
        {
            var entity = await repository.GetById(id);
            if (entity == null)
            {
                logger.LogInformation("GetFile {id} not found", id);
                return null;
            }

            var ret = mapper.Map<QFileServerModel>(entity);

            logger.LogInformation("GetFile {id} returning {fullFilePath}", id, entity.FullFilePath);
            return ret;
        }

        public async Task<QFileServerModel> AddFile(QFileServerModel model, Stream uploadedFileStream)
        {
            var destFilePath = await UploadToFs(uploadedFileStream, model.FileName);
            var fileSize = new FileInfo(destFilePath).Length;

            var entity = await repository.Create(new QFileServerEntity
            {
                FullFilePath = destFilePath,
                FileName = model.FileName,
                Size = fileSize
            });

            var ret = mapper.Map<QFileServerModel>(entity);

            logger.LogInformation("AddFile created record for {fileName} of size {fileSize} in {fullFilePath}",
                model.FileName, fileSize, destFilePath);
            return ret;
        }

        public async Task<bool> ReplaceFile(long id, QFileServerModel model, Stream uploadedFileStream)
        {
            var destFilePath = await UploadToFs(uploadedFileStream, model.FileName);
            var fileSize = new FileInfo(destFilePath).Length;

            var ret = await repository.Update(new QFileServerEntity
            {
                Id = id,
                FullFilePath = destFilePath,
                FileName = model.FileName,
                Size = fileSize
            });

            logger.LogInformation("ReplaceFile {updateSuccess} record id {id}, fileName {fileName} of size {fileSize} in {fullFilePath}",
                ret ? "updated" : "FAILED to update", id, model.FileName, fileSize, destFilePath);

            if (!ret)
            {
                File.Delete(destFilePath);
                logger.LogInformation("ReplaceFile deleted file id {id} from file system in {fullFilePath} (ROLLBACK)", id, destFilePath);
            }

            return ret;
        }

        public async Task<bool> DeleteFile(long id)
        {
            var entity = mapper.Map<QFileServerModel>(await repository.Delete(id));
            if (entity == null) {
                logger.LogInformation("DeleteFile no record found for {id}", id);
                return false;
            }
            logger.LogDebug("DeleteFile successfully deleted record {id}", id);

            File.Delete(entity.FullFilePath);
            logger.LogDebug("DeleteFile successfully deleted file {id} from file system in {fullFilePath}",
                id, entity.FullFilePath);
            
            logger.LogInformation("DeleteFile successfully removed {id} whose file was in {fullFilePath}",
                id, entity.FullFilePath);

            return true;
        }

        #region helpers
        async Task<string> UploadToFs(Stream uploadedFileStream, string fileName)
        {
            var destDir = Path.Combine(await storageDirectorySelector.GetStorageDirectory(fileName));
            logger.LogDebug("UploadToFs destDir {destDir} for fileName {fileName}", destDir, fileName);
            Directory.CreateDirectory(destDir);
            logger.LogDebug("UploadToFs ensured directory {destDir} is created", destDir);
            var randomFileName = Path.GetRandomFileName();
            var destFilePath = Path.Combine(destDir, randomFileName + "_" + fileName);
            logger.LogDebug("UploadToFs generated destination file path {destFilePath}", destFilePath);
            using (var destStream = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write))
                await uploadedFileStream.CopyToAsync(destStream);
            logger.LogDebug("UploadToFs successfully copied {fileName} to {destFilePath}", fileName, destFilePath);
            return destFilePath;
        }
        #endregion
    }
}
