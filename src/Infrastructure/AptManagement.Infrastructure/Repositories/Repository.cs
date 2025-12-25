using AptManagement.Domain.Common;
using AptManagement.Domain.Interfaces;
using AptManagement.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AptManagementContext db;

        public Repository(AptManagementContext repository)
        {
            db = repository;
        }

        public async Task CreateAsync(T entity)
        {   
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = 1;
            await db.Set<T>().AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            ArgumentNullException.ThrowIfNull(entity);
            entity.DeletedDate =
                TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));
            entity.DeletedBy = 1;
            db.Entry(entity).State = EntityState.Modified;
            db.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
            db.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
            db.Entry(entity).Property(x => x.UpdatedBy).IsModified = false;
            db.Entry(entity).Property(x => x.UpdatedDate).IsModified = false;
            db.SaveChanges();
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] properties)
        {
            var query = db.Set<T>().AsNoTracking();
            if (properties.Length != 0)
                query = properties.Aggregate(query, (current, property) => current.Include(property));
            return query;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return await db.Set<T>().FindAsync(id);
        }

        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.UpdatedDate =
                TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));
            entity.UpdatedBy = 1;
            db.Entry(entity).State = EntityState.Modified;
            db.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
            db.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
            db.SaveChanges();
        }
    }
}
