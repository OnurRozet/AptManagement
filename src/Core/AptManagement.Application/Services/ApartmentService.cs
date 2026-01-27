using AptManagement.Application.Common;
using AptManagement.Application.Common.Base;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Extensions;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Enums;
using AptManagement.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AptManagement.Application.Services
{
    public class ApartmentService(IUnitOfWork unitOfWork, IRepository<Apartment> repository, IRepository<ApartmentDebt> debtRepo ,IMapper mapper, IValidator<Apartment> validator) : IApartmentService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ApartmentDto apartment)
        {
            Apartment newApartment = mapper.Map<Apartment>(apartment);
            //validasyon ekle
            var validationResult = validator.Validate(newApartment);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (newApartment == null) return ServiceResult<CreateOrEditResponse>.Error();

            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {

                if (newApartment.Id > 0)
                {
                    repository.Update(newApartment);
                    return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = newApartment.Id }, "Başarılı şekilde güncellenmiştir.");
                }

                await repository.CreateAsync(newApartment);

                if(newApartment.OpeningBalance < 0)
                {
                    await CreateTransferDebtAsync(newApartment.Id, newApartment.OpeningBalance);
                }

                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = newApartment.Id }, "Başarılı şekilde oluşturulmuştur.");
            });
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
             {
                 var apartment = await repository.GetByIdAsync(id);
                 if (apartment == null) return false;
                 repository.Delete(apartment);
                 return true;

             });
        }

        public async Task<ServiceResult<SearchResponse<ApartmentResponse>>> Search(ApartmentSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.ApartmentId.HasValue, x => x.Id == request.ApartmentId.Value)
                .WhereIf(!string.IsNullOrEmpty(request.TenantName), x => x.TenantName == request.TenantName)
                .Select(x => new ApartmentResponse()
                {
                    Id = x.Id,
                    Label = x.Label,
                    OwnerName = x.OwnerName,
                    TenantName = x.TenantName,
                    Balance = x.Balance,
                    OpeningBalance= x.Debts.Where(x=>x.DebtType == DebtType.TransferFromPast).Sum(x=>x.Amount - x.PaidAmount),
                })
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<ApartmentResponse>>.Success(new SearchResponse<ApartmentResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }

        public async Task<ServiceResult<DetailResponse<Apartment>>> GetApartmentById(int id)
        {
            var apartment = await repository.GetByIdAsync(id);

            if (apartment == null) return ServiceResult<DetailResponse<Apartment>>.Error("Belirtilen id ye sahip bir daire bulunamadı");

            return ServiceResult<DetailResponse<Apartment>>.Success(new DetailResponse<Apartment> { Detail = apartment });
        }

        public async Task<ServiceResult<bool>> SetOpeningBalanceAsync(int apartmentId, decimal amount)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. Daireyi bul ve açılış bakiyesini güncelle
                var apartment = await repository.GetByIdAsync(apartmentId);
                if (apartment == null) return ServiceResult<bool>.Error("Daire bulunamadı.");

                apartment.OpeningBalance = amount;
                repository.Update(apartment);

                // 2. Eğer tutar negatifse (borç demek), bunu bir ApartmentDebt olarak kaydet
                if (amount < 0)
                {
                    var transferDebt = new ApartmentDebt
                    {
                        ApartmentId = apartmentId,
                        Amount = Math.Abs(amount), // Pozitif borç tutarı
                        DebtType = DebtType.TransferFromPast,
                        DueDate = new DateTime(DateTime.Now.Year, 1, 1), // Yılın başı veya sistem kurulum tarihi
                        Description = "Geçmiş Dönemden Devreden Borç",
                        IsClosed = false,
                        PaidAmount = 0
                    };
                    await debtRepo.CreateAsync(transferDebt);
                }

                return ServiceResult<bool>.Success(result:true,"Açılış bakiyesi ve devir borcu tanımlandı.");
            });
        }

        public async Task<ServiceResult<CreateOrEditResponse>> CreateApartmentsBulkAsync(List<ApartmentDto> request)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var apartments = mapper.Map<List<Apartment>>(request);

                // 1. Önce Dairelerin Balance değerini ayarla (Borçları sonra tabloya atacağız)
                foreach (var apt in apartments)
                {
                    // Eğer OpeningBalance pozitifse (+), bu bir alacaktır (fazla para).
                    // Eğer negatifse (-), bu bir borçtur, Balance 0 kalmalıdır (borç ApartmentDebt'e gidecek).
                    apt.Balance = apt.OpeningBalance > 0 ? apt.OpeningBalance : 0;
                }

                // 2. Daireleri ekle (ID'lerin oluşması için bu satır şart)
                await repository.BulkCreateAsync(apartments);

                // KRİTİK NOT: Eğer veritabanı ID'leri geri dönmüyorsa burada SaveChanges veya 
                // kütüphaneye özel "IncludeGraph" gibi ayarlar gerekebilir.

                // 3. Borçları hazırla
                var initialDebts = new List<ApartmentDebt>();
                foreach (var apt in apartments)
                {
                    if (apt.OpeningBalance < 0)
                    {
                        initialDebts.Add(new ApartmentDebt
                        {
                            ApartmentId = apt.Id, // Buranın 0 olmadığından emin ol!
                            Amount = Math.Abs(apt.OpeningBalance),
                            DebtType = DebtType.TransferFromPast,
                            DueDate = new DateTime(DateTime.Now.Year, 1, 1),
                            Description = "Geçmiş dönem devir borcu (Toplu Giriş)",
                            IsClosed = false,
                            PaidAmount = 0
                        });
                    }
                }

                // 4. Borçları ekle
                if (initialDebts.Any())
                {
                    await debtRepo.BulkCreateAsync(initialDebts);
                }

                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse(), $"{apartments.Count} daire ve borç kayıtları başarıyla sisteme entegre edildi.");
            });
        }

        /// <summary>
        /// Dairenin geçmişten gelen borç bakiyesini sisteme borç kaydı olarak işler.
        /// </summary>
        /// <param name="apartmentId">İlgili dairenin ID'si</param>
        /// <param name="openingBalance">Açılış bakiyesi (Eksi değer borç, artı değer alacak demektir)</param>
        private async Task CreateTransferDebtAsync(int apartmentId, decimal openingBalance)
        {
            // 1. Daireyi kontrol et
            var apartment = await repository.GetByIdAsync(apartmentId);
            if (apartment == null) return;

            // 2. Önce bu daireye ait eski "Devir Borcu" var mı diye bak
            var existingTransferDebt = await debtRepo.GetAll()
                .FirstOrDefaultAsync(x => x.ApartmentId == apartmentId && x.DebtType == DebtType.TransferFromPast);

            // 3. Senaryo A: Alacak durumu (openingBalance >= 0)
            if (openingBalance >= 0)
            {
                apartment.Balance = openingBalance;
                repository.Update(apartment);

                // Eğer önceden bir devir borcu varsa ve bakiye artık artıya döndüyse, eski borcu sil!
                if (existingTransferDebt != null)
                {
                    debtRepo.Delete(existingTransferDebt);
                }
                return;
            }
            // 4. Senaryo B: Borç durumu (openingBalance < 0)
            decimal debtAmount = Math.Abs(openingBalance);

            if (existingTransferDebt != null)
            {
                // Varsa silmek yerine GÜNCELLEMEK daha performanslıdır (ID değişmez)
                existingTransferDebt.Amount = debtAmount;
                existingTransferDebt.PaidAmount = 0; // Bakiye sıfırdan girildiği için ödeneni sıfırlıyoruz
                existingTransferDebt.IsClosed = false;
                existingTransferDebt.Description = "Sistem açılışında GÜNCELLENEN geçmiş dönem devir borcu.";
                debtRepo.Update(existingTransferDebt);
            }
            else
            {
                // Yoksa YENİ oluştur
                var transferDebt = new ApartmentDebt
                {
                    ApartmentId = apartmentId,
                    Amount = debtAmount,
                    PaidAmount = 0,
                    DebtType = DebtType.TransferFromPast,
                    DueDate = new DateTime(DateTime.Now.Year, 1, 1),
                    Description = "Sistem açılışında girilen geçmiş dönem devir borcu.",
                    IsClosed = false,
                };
                await debtRepo.CreateAsync(transferDebt);
            }

            // Dairenin Balance alanını borç durumunda 0 yapalım (Çünkü borç artık ApartmentDebt tablosunda)
            apartment.Balance = 0;
            repository.Update(apartment);
        }


        public async Task<ServiceResult<List<ApartmentDto>>> ParseApartmentExcelAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length <= 0)
                return ServiceResult<List<ApartmentDto>>.Error("Lütfen geçerli bir Excel dosyası yükleyin.");

            var list = new List<ApartmentDto>();

            try
            {
                // EPPlus lisans bağlamı (Ücretsiz kullanım için NonCommercial seçilmeli)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0; // Stream'i başa al

                    using (var package = new ExcelPackage(stream))
                    {
                        // Workbook kontrolü
                        if (package.Workbook == null || package.Workbook.Worksheets == null || package.Workbook.Worksheets.Count == 0)
                            return ServiceResult<List<ApartmentDto>>.Error("Excel dosyasında çalışma sayfası bulunamadı.");

                        // İlk sayfayı al
                        var worksheet = package.Workbook.Worksheets[0];
                        
                        // Dimension kontrolü
                        if (worksheet.Dimension == null)
                            return ServiceResult<List<ApartmentDto>>.Error("Excel dosyası boş görünüyor.");

                        var rowCount = worksheet.Dimension.Rows;

                        // 1. satırın başlık olduğunu varsayıyoruz, 2'den başlıyoruz
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var labelValue = worksheet.Cells[row, 1].Value?.ToString();
                                var openingBalanceValue = worksheet.Cells[row, 4].Value?.ToString();

                                // Label boşsa bu satırı atla
                                if (string.IsNullOrWhiteSpace(labelValue))
                                    continue;

                                // OpeningBalance parse işlemi (güvenli)
                                if (!decimal.TryParse(openingBalanceValue, out decimal openingBalance))
                                    openingBalance = 0;

                                var dto = new ApartmentDto
                                {
                                    Label = labelValue.Trim(), // A Kolonu: Daire No/Etiket
                                    OwnerName = worksheet.Cells[row, 2].Value?.ToString()?.Trim(), // B Kolonu: Sakin Adı
                                    TenantName = worksheet.Cells[row, 3].Value?.ToString()?.Trim(), // C Kolonu: Kiracı Adı
                                    OpeningBalance = openingBalance, // D Kolonu: Borç/Alacak
                                    IsManager = worksheet.Cells[row, 5].Value?.ToString()?.Trim().Equals("Evet", StringComparison.OrdinalIgnoreCase) ?? false, // E Kolonu: Yönetici mi
                                };

                                list.Add(dto);
                            }
                            catch (Exception ex)
                            {
                                // Hatalı satırı atla ve devam et
                                // Loglama yapılabilir: $"Satır {row} işlenirken hata: {ex.Message}"
                                continue;
                            }
                        }

                        await CreateApartmentsBulkAsync(list);
                    }
                }

                if (list.Count == 0)
                    return ServiceResult<List<ApartmentDto>>.Error("Excel dosyasında işlenecek veri bulunamadı. Lütfen dosya formatını kontrol edin.");

                return ServiceResult<List<ApartmentDto>>.Success(list);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ApartmentDto>>.Error($"Excel dosyası okunurken bir hata oluştu: {ex.Message}");
            }
        }
    }
}
