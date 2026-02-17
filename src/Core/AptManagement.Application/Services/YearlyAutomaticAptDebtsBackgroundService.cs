using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AptManagement.Application.Services
{
    public class YearlyAutomaticAptDebtsBackgroundService(IServiceProvider serviceProvider, ILogger<YearlyAutomaticAptDebtsBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Yıllık Borçlandırma Servisi Başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var apartmentRepo = scope.ServiceProvider.GetRequiredService<IRepository<Apartment>>();
                        var debtRepo = scope.ServiceProvider.GetRequiredService<IRepository<ApartmentDebt>>();
                        var dueSettings = scope.ServiceProvider.GetRequiredService<IRepository<DuesSetting>>();
                        var managementPeriod = scope.ServiceProvider.GetRequiredService<IRepository<ManagementPeriod>>();

                        int currentYear = DateTime.Now.Year;
                       //int currentYear = 2025;

                        // 1. Tüm daireleri çek
                        var apartments = await apartmentRepo.GetAll().ToListAsync();

                        foreach (var apt in apartments)
                        {
                            // 2. Bu daire için bu yıla ait borçlar zaten oluşturulmuş mu?
                            bool isAlreadyCreated = await debtRepo.GetAll()
                               .AnyAsync(x => x.ApartmentId == apt.Id && x.DueDate.Year == currentYear && x.DebtType == Domain.Enums.DebtType.Dues);

                            //3. Sistem özelinde tanımlanmış olan aidat tutarını çek.
                            var due = await dueSettings.GetAll().Where(x => x.StartDate.Year == currentYear).FirstOrDefaultAsync();

                            if (due == null) continue;


                            if (!isAlreadyCreated)
                            {
                                var debts = new List<ApartmentDebt>();
                                // Daire için bu yılla kesişen aktif yönetim dönemi (IsExemptFromDues'a göre muafiyet)
                                var periodForApt = await managementPeriod.GetAll()
                                    .Where(x => x.ApartmentId == apt.Id && x.IsActive &&
                                                x.StartDate.Year <= currentYear &&
                                                (x.EndDate == null || x.EndDate.Value.Year >= currentYear))
                                    .FirstOrDefaultAsync();

                                for (int month = 1; month <= 12; month++)
                                {
                                    var dueDate = month == 2 ? new DateTime(currentYear, month, 28) : new DateTime(currentYear, month, 30);

                                    // Aidattan muaf mı: EndDate'in olduğu ay dahil muaf (EndDate 15 Haziran ise Haziran tamamen muaf)
                                    var effectiveEndDate = periodForApt.EndDate.HasValue
                                        ? new DateTime(periodForApt.EndDate!.Value.Year, periodForApt.EndDate.Value.Month, DateTime.DaysInMonth(periodForApt.EndDate.Value.Year, periodForApt.EndDate.Value.Month))
                                        : new DateTime(currentYear, 12, 31);
                                    bool isExempt = periodForApt != null &&
                                                    periodForApt.IsExemptFromDues &&
                                                    dueDate >= periodForApt.StartDate &&
                                                    dueDate <= effectiveEndDate;

                                    if (isExempt)
                                    {
                                        debts.Add(new ApartmentDebt
                                        {
                                            ApartmentId = apt.Id,
                                            Amount = 0,
                                            DueDate = dueDate,
                                            IsClosed = false,
                                            CreatedDate = DateTime.Now,
                                            Description = "Yönetici Muafiyeti"
                                        });
                                    }
                                    else
                                    {
                                        debts.Add(new ApartmentDebt
                                        {
                                            ApartmentId = apt.Id,
                                            Amount = due.Amount,
                                            DueDate = dueDate,
                                            IsClosed = false,
                                            CreatedDate = DateTime.Now,
                                            Description = $"{month}. ay aidat borcu"
                                        });
                                    }
                                }

                                logger.LogInformation("Aidat borçları toplandı");

                                foreach (var item in debts)
                                {
                                    await debtRepo.CreateAsync(item);
                                }
                                logger.LogInformation("Aidat borçları oluşturuldu");
                            }
                        }
                    }
                    // Her 24 saatte bir kontrol et
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "Borçlandırma işlemi sırasında hata oluştu.");
                }

            }
        }
    }
}
