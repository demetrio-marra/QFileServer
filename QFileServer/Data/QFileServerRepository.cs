using Microsoft.EntityFrameworkCore;
using QFileServer.Data.Entities;

namespace QFileServer.Data
{
    public class QFileServerRepository
    {
        private readonly QFileServerDbContext context;

        public QFileServerRepository(QFileServerDbContext dbContext)
        {
            context = dbContext;
        }

        public IQueryable<QFileServerEntity> GetAllOData()
        {
            return context.Set<QFileServerEntity>().AsNoTracking()
                .AsQueryable();
        }

        public async Task<IEnumerable<QFileServerEntity>> GetAll()
            => await context.Set<QFileServerEntity>().AsNoTracking().ToListAsync();

        public async Task<QFileServerEntity?> GetById(long id)
            =>  await context.Set<QFileServerEntity>().FindAsync(id);

        public async Task<QFileServerEntity> Create(QFileServerEntity entity)
        {
            var r = await context.Set<QFileServerEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            return r.Entity;
        }

        public async Task<QFileServerEntity?> Delete(long id)
        {
            var objEntity = await GetById(id);
            if (objEntity == null)
                return null;

            context.Set<QFileServerEntity>().Remove(objEntity);
            await context.SaveChangesAsync();

            return objEntity;
        }

        public async Task<bool> Update(QFileServerEntity entity)
        {
            var objEntity = await GetById(entity.Id);
            if (objEntity == null)
                return false;
            
            objEntity.FullFilePath = entity.FullFilePath;
            objEntity.Size = entity.Size;
            objEntity.FileName = entity.FileName;

            context.Set<QFileServerEntity>().Update(objEntity);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
