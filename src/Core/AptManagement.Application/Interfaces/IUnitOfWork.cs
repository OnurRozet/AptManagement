using System;
using System.Threading;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    /// <summary>
    /// Uygulama genelinde transaction ve iş birimi (Unit Of Work) yönetimi sağlar.
    /// Repository'ler aynı DbContext üzerinden çalıştığı için, burada sadece
    /// transaction yaşam döngüsünü soyutluyoruz.
    /// </summary>
    public interface IUnitOfWork
    {
        
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    }
}


