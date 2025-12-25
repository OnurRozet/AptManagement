using AptManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        var apartments = new List<Apartment>
        {
            new Apartment { Id = 1, Label="Dükkan", OwnerName="Fatma Uğur", TenantName="Kaan Ersöz (Getir Su Bayii)", Balance=0 },
            new Apartment { Id = 2, Label="Daire 2", OwnerName="Eray Şişman", TenantName="Begümsu Güller", Balance=0 },
            new Apartment { Id = 3, Label="Daire 3", OwnerName="İhsan Esen", TenantName="Serhat Vardar", Balance=0 },
            new Apartment { Id = 4, Label="Daire 4", OwnerName="Şaban Karadeniz", TenantName="Hasan Alcay", Balance=0 },
            new Apartment { Id = 5, Label="Daire 5", OwnerName="İlknur Oran", TenantName="", Balance=0 },
            new Apartment { Id = 6, Label="Daire 6", OwnerName="Şenol Baykal", TenantName="", Balance=0 },
            new Apartment { Id = 7, Label="Daire 7", OwnerName="Kadir Özer", TenantName="", Balance=0 },
            new Apartment { Id = 8, Label="Daire 8", OwnerName="Abdulkadir Cebecioğlu", TenantName="", Balance=0 },
            new Apartment { Id = 9, Label="Daire 9", OwnerName="Onur Rözet", TenantName="", Balance=0, IsManager=true },
            new Apartment { Id = 10, Label="Daire 10", OwnerName="Abdulkadir Cebecioğlu", TenantName="", Balance=0 },
            new Apartment { Id = 11, Label="Daire 11", OwnerName="Cem Bey", TenantName="Reyhan Özer", Balance=0 },
            new Apartment { Id = 12, Label="Daire 12", OwnerName="Erkan Şimşek", TenantName="Kiracı", Balance=0 },
            new Apartment { Id = 13, Label="Daire 13", OwnerName="Menekşe Yalçın", TenantName="Enes Şengül", Balance=0 },
            new Apartment { Id = 14, Label="Daire 14", OwnerName="Emine Erten", TenantName="", Balance=0 },
        };

        builder.HasData(apartments);
    }
}