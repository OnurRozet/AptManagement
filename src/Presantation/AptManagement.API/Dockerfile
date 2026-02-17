# -----------------------------------------------------------------------
# 1. BASE AÞAMASI: Uygulamanýn çalýþacaðý hafif çalýþma ortamý
# -----------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# .NET 9 için varsayýlan modern portlar
EXPOSE 8080
EXPOSE 8081

# -----------------------------------------------------------------------
# 2. BUILD AÞAMASI: Derleme ve Baðýmlýlýklarýn Çözülmesi (SDK gerekir)
# -----------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Proje dosyalarýný tek tek kopyalýyoruz (Docker Cache dostu yaklaþým)
# Not: Yollarý senin yapýna göre (src/...) korudum ama WORKDIR karmaþasýný sildim.
COPY ["src/Presantation/AptManagement.API/AptManagement.API.csproj", "src/Presantation/AptManagement.API/"]
COPY ["src/Core/AptManagement.Application/AptManagement.Application.csproj", "src/Core/AptManagement.Application/"]
COPY ["src/Core/AptManagement.Domain/AptManagement.Domain.csproj", "src/Core/AptManagement.Domain/"]
COPY ["src/Infrastructure/AptManagement.Infrastructure/AptManagement.Infrastructure.csproj", "src/Infrastructure/AptManagement.Infrastructure/"]

# Baðýmlýlýklarý indiriyoruz
RUN dotnet restore "./src/Presantation/AptManagement.API/AptManagement.API.csproj"

# Tüm kodlarý kopyalýyoruz
COPY . .

# Derleme adýmýna geçiyoruz - Doðrudan ilgili klasöre odaklanýyoruz
WORKDIR "/src/src/Presantation/AptManagement.API"
RUN dotnet build "./AptManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -----------------------------------------------------------------------
# 3. PUBLISH AÞAMASI: Uygulamayý yayýna hazýrlama (DLL'leri toplama)
# -----------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AptManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------
# 4. FINAL AÞAMASI: Sadece gerekli dosyalarý alýp ayaða kalkma
# -----------------------------------------------------------------------
FROM base AS final
WORKDIR /app
# Sadece publish edilen dosyalarý çekiyoruz, 2GB'lýk SDK geride kalýyor.
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AptManagement.API.dll"]