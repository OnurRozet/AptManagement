using AptManagement.Domain.Entities;
using AptManagement.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDependencies(builder.Configuration);

#region CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowPolicy", x =>
    {
        x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});
#endregion


var app = builder.Build();

#region Migrate

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var context = services.GetRequiredService<AptManagement.Infrastructure.Context.AptManagementContext>();

    try
    {
        logger.LogInformation("Veritabanı migration işlemi başlatılıyor...");

        // Sert Uyarı: Docker'da SQL Server hemen hazır olmaz. 
        // Basit bir retry (yeniden deneme) mantığı eklemek hayat kurtarır.
        int retryCount = 0;
        bool isDbReady = false;

        while (!isDbReady && retryCount < 10)
        {
            try
            {
                // EF Core'un sihirli değneği: 
                // Tablolar yoksa oluşturur, varsa sadece eksik migration'ları basar.
                context.Database.Migrate();
                isDbReady = true;
                //await SeedDataAsync(userManager, roleManager, logger);

                logger.LogInformation("Veritabanı ve başlangıç verileri (Seed) hazır.");
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogWarning($"Veritabanına bağlanılamadı (Deneme {retryCount}/10). Bekleniyor...");
                // SQL Server'ın kendine gelmesi için 5 saniye bekle
                Thread.Sleep(5000);

                if (retryCount >= 10)
                {
                    logger.LogCritical(ex, "Veritabanı bağlantısı 10 deneme sonrası başarısız oldu!");
                    throw;
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration sırasında bir hata oluştu.");
    }
}

#endregion

//#region Seed Data
//async Task SeedDataAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
//{
//    string adminRole = "Admin";
//    string adminApartmentNumber = "9";
//    string adminPassword = "123456"; // Sert Uyarı: Güçlü şifre politikasına uymalı!

//    // Rolü kontrol et ve yoksa oluştur
//    if (!await roleManager.RoleExistsAsync(adminRole))
//    {
//        await roleManager.CreateAsync(new IdentityRole<Guid> { Name = adminRole });
//        logger.LogInformation($"'{adminRole}' rolü oluşturuldu.");
//    }

//    // Kullanıcıyı kontrol et
//    var defaultUser = await userManager.FindByNameAsync(adminApartmentNumber);

//    if (defaultUser == null)
//    {
//        var newAdmin = new AppUser
//        {
//            Id = Guid.NewGuid(),
//            UserName = "admin",
//            Email = "adminsite@adminsite.com.tr",
//            EmailConfirmed = true,
//            FullName = "Onur Rozet",
//            ApartmentId = 9,
//            ApartmentNumber = adminApartmentNumber,
//            IsManager = true,

//        };

//        var result = await userManager.CreateAsync(newAdmin, adminPassword);

//        if (result.Succeeded)
//        {
//            await userManager.AddToRoleAsync(newAdmin, adminRole);
//            logger.LogInformation($"Başlangıç kullanıcısı ({adminApartmentNumber}) başarıyla oluşturuldu.");
//        }
//        else
//        {
//            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//            logger.LogError($"Kullanıcı oluşturulamadı! Hatalar: {errors}");
//        }
//    }
//}
//#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowPolicy");

// HTTPS redirection sadece Production ortamında aktif olsun
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
