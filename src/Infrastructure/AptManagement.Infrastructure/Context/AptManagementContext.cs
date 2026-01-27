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
    public class AptManagementContext : IdentityDbContext<AppUser, IdentityRole<Guid>,Guid>
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
            modelBuilder.ApplyConfiguration(new ApartmentConfiguration());
        }

    }
}
