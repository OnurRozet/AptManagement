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

                        int currentYear = DateTime.Now.Year;

                        // 1. Tüm daireleri çek
                        var apartments = await apartmentRepo.GetAll().ToListAsync();

                        foreach (var apt in apartments)
                        {
                            // 2. Bu daire için bu yıla ait borçlar zaten oluşturulmuş mu?
                            bool isAlreadyCreated = await debtRepo.GetAll()
                               .AnyAsync(x => x.ApartmentId == apt.Id && x.CreatedDate.Year == currentYear);

                            //3. Sistem özelinde tanımlanmış olan aidat tutarını çek.
                            var due = await dueSettings.GetAll().Where(x => x.StartDate.Year == currentYear).FirstOrDefaultAsync();

                            if (due == null) continue;

                            if (!isAlreadyCreated)
                            {
                                // 3. 12 aylık borç kaydı oluştur
                                var debts = new List<ApartmentDebt>();
                                for (int month = 1; month <= 12; month++)
                                {
                                    debts.Add(new ApartmentDebt
                                    {
                                        ApartmentId = apt.Id,
                                        Amount = apt.IsManager ? 0 : due.Amount, // Daire tablosunda tanımlı varsayılan aidat
                                        DueDate = month == 2 ? new DateTime(currentYear, month, 28) : new DateTime(currentYear, month, 30),
                                        IsClosed = false,
                                        CreatedDate = DateTime.Now,
                                        Description = $"{month}. ay aidat borcu"
                                    });
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
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "Borçlandırma işlemi sırasında hata oluştu.");
                }

            }
        }
    }
}
