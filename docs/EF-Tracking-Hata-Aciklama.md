# EF Core "Cannot be tracked - another instance with the same key" Hatası

## Hatanın Tam Metni
```
System.InvalidOperationException: The instance of entity type 'ApartmentDebt' cannot be tracked 
because another instance with the same key value for {'Id'} is already being tracked.
```

## Oluşma Nedenleri

### 1. Aynı entity'nin farklı instance'larının aynı anda takip edilmesi
EF Core Change Tracker, aynı primary key'e sahip yalnızca **tek bir** entity instance'ını takip edebilir. Aynı Id'li iki farklı nesne (instance) oluşturulup ikisi de tracker'a eklenmeye çalışılırsa bu hata oluşur.

### 2. Senaryomuzdaki spesifik durum: Gelir güncelleme akışı

**Akış:**
1. `RevertIncomeAllocationsAsync`: Eski dairenin borçlarını (ApartmentDebt) günceller
   - `allocationDebtRepo.GetAll(a => a.ApartmentDebt)` ile allocation ve borçlar yüklenir (AsNoTracking)
   - Her borç için `debt.PaidAmount -= ...` yapılıp `debtRepo.Update(debt)` çağrılır
   - `Update` metodu `db.Entry(entity).State = Modified` yaparak entity'yi **tracker'a ekler (attach)**

2. `DistributePaymentToDebtsAsync`: (Aynı veya farklı) dairenin borçlarına yeni ödeme dağıtır
   - `debtRepo.GetAll().Where(...)` ile borçlar **yeniden sorgulanır**
   - `GetAll` **AsNoTracking** kullandığı için aynı kayıtlar için **yeni, farklı instance'lar** döner
   - `debtRepo.Update(debt)` çağrıldığında EF aynı Id'li başka bir instance zaten takip edildiği için **hata fırlatır**

### 3. Ne zaman tetiklenir?
- **Kullanıcı geliri güncellerken daireyi değiştirmese** (sadece tutar/tarih değişse): Aynı dairenin aynı borçları hem Revert'ta hem Distribute'ta işlenir → **çakışma**
- **Kullanıcı geliri farklı bir daireye taşısa**: Genelde farklı borçlar işlenir, çakışma olmaz (nadir edge case'ler hariç)

### 4. Özet
| Adım | Kaynak | ApartmentDebt instance'ları | Tracker durumu |
|------|--------|----------------------------|----------------|
| Revert | allocation.ApartmentDebt | Instance A (Id=5) | A tracker'da |
| Distribute | debtRepo.GetAll() | Instance B (Id=5) | B'yi attach etmeye çalış → **Çakışma** (Id=5 zaten A ile takipte) |

## Çözüm

`Repository.Update` metodunda: Entity zaten tracker'da varsa, yeni instance'ı attach etmek yerine **tracked entity'nin değerlerini güncelle**. Böylece aynı Id'li iki instance tracker'a girmeye çalışmaz.

### Uygulanan değişiklik (Repository.cs)

```csharp
var trackedEntity = db.Set<T>().Local.FirstOrDefault(e => e.Id == entity.Id);
if (trackedEntity != null)
{
    // Aynı Id'li entity zaten tracker'da - tracked entity'nin değerlerini kopyala
    db.Entry(trackedEntity).CurrentValues.SetValues(entity);
}
else
{
    db.Entry(entity).State = EntityState.Modified;
}
db.SaveChanges();
```

Bu sayede `RevertIncomeAllocationsAsync` ve `DistributePaymentToDebtsAsync` aynı borçları işlediğinde (aynı daire, sadece tutar güncellemesi) hata oluşmaz.
