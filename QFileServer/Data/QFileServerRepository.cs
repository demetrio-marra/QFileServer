using Microsoft.EntityFrameworkCore;
using Polly;
using QFileServer.Configuration.Resilience;
using QFileServer.Data.Entities;

namespace QFileServer.Data
{
    public class QFileServerRepository
    {
        private readonly QFileServerDbContext context;
        private readonly IAsyncPolicy sqlPolicy;

        public QFileServerRepository(QFileServerDbContext dbContext, IResilientPoliciesLocator policiesLocator)
        {
            context = dbContext;
            sqlPolicy = policiesLocator.GetPolicy(ResilientPolicyType.SqlDatabase);
        }

        public IQueryable<QFileServerEntity> GetAllOData()
        {
            return context.Set<QFileServerEntity>().AsNoTracking()
                .AsQueryable();
        }

        public async Task<IEnumerable<QFileServerEntity>> GetAll()
            => await sqlPolicy.ExecuteAsync(async () => await context.Set<QFileServerEntity>().AsNoTracking().ToListAsync());

        public async Task<QFileServerEntity?> GetById(long id)
            => await sqlPolicy.ExecuteAsync(async () => await context.Set<QFileServerEntity>().FindAsync(id));

        public async Task<QFileServerEntity> Create(QFileServerEntity entity)
        {
            var ret = await sqlPolicy.ExecuteAsync(async () =>
            {
                var r = await context.Set<QFileServerEntity>().AddAsync(entity);
                await context.SaveChangesAsync();
                return r;
            });

            return ret.Entity;
        }

        public async Task<QFileServerEntity?> Delete(long id)
        {
            var ret = await sqlPolicy.ExecuteAsync(async () =>
            {
                var objEntity = await GetById(id);
                if (objEntity == null)
                    return null;

                context.Set<QFileServerEntity>().Remove(objEntity);
                await context.SaveChangesAsync();

                return objEntity;
            });

            return ret;
        }

        public async Task<bool> Update(QFileServerEntity entity)
        {
            var ret = await sqlPolicy.ExecuteAsync(async () =>
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
            });

            return ret;
        }
    }
}
