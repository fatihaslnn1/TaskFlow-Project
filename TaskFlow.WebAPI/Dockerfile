# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyalayıp bağımlılıkları yüklüyoruz
COPY ["TaskFlow.WebAPI/TaskFlow.WebAPI.csproj", "TaskFlow.WebAPI/"]
COPY ["TaskFlow.Application/TaskFlow.Application.csproj", "TaskFlow.Application/"]
COPY ["TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]
COPY ["TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
RUN dotnet restore "TaskFlow.WebAPI/TaskFlow.WebAPI.csproj"

# Kodun tamamını kopyalayıp derliyoruz
COPY . .
WORKDIR "/src/TaskFlow.WebAPI"
RUN dotnet build "TaskFlow.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final (Çalıştırma) aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.WebAPI.dll"]