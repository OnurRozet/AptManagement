using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task CreateAsync(T entity);
        void Delete(T entity);
        void Update(T entity);

        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] properties);
    }
}
