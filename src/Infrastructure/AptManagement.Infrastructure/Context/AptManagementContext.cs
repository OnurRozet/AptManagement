using AptManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Infrastructure.Context
{
    public class AptManagementContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AptManagementContext(DbContextOptions<AptManagementContext> options) : base(options)
        {
        }

        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<ApartmentDebt> ApartmentDebts { get; set; }
        public DbSet<DuesSetting> DuesSettings { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<IncomeCategory> IncomeCategories { get; set; }
        public DbSet<ManagementPeriod> ManagementPeriods { get; set; }
        public DbSet<IncomeDebtAllocation> IncomeDebtAllocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            var decimalProperties = modelBuilder.Model.GetEntityTypes()
           .SelectMany(t => t.GetProperties())
           .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in decimalProperties)
            {
                property.SetPrecision(18);
                property.SetScale(2);
                // Veya alternatif olarak: property.SetColumnType("decimal(18,2)");
            }
            //modelBuilder.ApplyConfiguration(new ApartmentConfiguration());

            // BaseEntity alanları dahil snapshot ile birebir eşleşmeli (PendingModelChanges uyarısını önler)
            modelBuilder.Entity<Apartment>().HasData(new
            {
                Id = 9,
                Label = "Daire 9",
                OwnerName = "Onur Rozet",
                TenantName = (string?)null,
                Balance = 0m,
                OpeningBalance = 0m,
                IsManager = true,
                CreatedBy = 0,
                CreatedDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                UpdatedBy = 0,
                UpdatedDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                DeletedBy = 0,
                DeletedDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                IsDeleted = false
            });

            var adminId = Guid.Parse("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D");
            var roleId = Guid.Parse("B2C3D4E5-F6A7-5B6C-9D0E-1F2A3B4C5D6E");

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(new IdentityRole<Guid>
            {
                Id = roleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "1" // Sabit değer
            });

            // ÖNEMLİ: HasData'da SADECE sabit değerler kullanılmalı!
            // hasher.HashPassword() her seferinde farklı hash üretir (random salt) -> PendingModelChangesWarning
            // Bu hash "123456" şifresine ait önceden hesaplanmış sabit değerdir.
            modelBuilder.Entity<AppUser>().HasData(new AppUser
            {
                Id = adminId,
                UserName = "9",
                NormalizedUserName = "9", // FindByNameAsync NormalizedUserName ile arar; "9" ile arama yapılacak
                Email = "admin@siteyonetimi.com",
                NormalizedEmail = "ADMIN@SITEYONETIMI.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEN5dvYMMZR6e5oz/fyUG3oXS13RiNNwFOvZf2iJQsQ+mham0nOtxYU0uBqjwHKIQhg==",
                SecurityStamp = "A7D5C6E8-B9F2-4E1D-8C9A-0B1C2D3E4F5G",
                ConcurrencyStamp = "1", // Sabit değer
                FullName = "Onur Rozet",
                IsManager = true,
                ApartmentId = 9,
                ApartmentNumber = "9"
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
            {
                RoleId = roleId,
                UserId = adminId
            });

            // SQL Server: Multiple cascade path hatasını önlemek için Income->IncomeDebtAllocation ilişkisinde NoAction
            modelBuilder.Entity<IncomeDebtAllocation>()
                .HasOne(x => x.Income)
                .WithMany(x => x.Allocations)
                .HasForeignKey(x => x.IncomeId)
                .OnDelete(DeleteBehavior.NoAction);
        }


    }
}
