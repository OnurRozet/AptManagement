using AptManagement.Application.Interfaces;
using AptManagement.Infrastructure.Context;

namespace AptManagement.Infrastructure.UnitOfWork
{
    /// <summary>
    /// EF Core DbContext üzerinde transaction yönetimini üstlenen UnitOfWork implementasyonu.
    /// Repository'ler aynı DbContext örneğini kullandığı için,
    /// burada sadece Database transaction'ı yönetiyoruz.
    /// </summary>
    public class UnitOfWork(AptManagementContext context) : IUnitOfWork
    {
        private readonly AptManagementContext _context = context;

        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            // Zaten bir transaction varsa tekrar oluşturmadan doğrudan aksiyonu çalıştır.
            if (_context.Database.CurrentTransaction != null)
            {
                await action();
                return;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                return await action();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await action();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}


