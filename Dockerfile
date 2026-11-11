# Етап 1: Збірка (Build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копіюємо файл проєкту та відновлюємо NuGet-пакети
COPY ["src/Karpaty.Api/Karpaty.Api.csproj", "src/Karpaty.Api/"]
RUN dotnet restore "src/Karpaty.Api/Karpaty.Api.csproj"

# Копіюємо весь код бекенду та публікуємо у Release
COPY ["src/Karpaty.Api/", "src/Karpaty.Api/"]
WORKDIR "/src/src/Karpaty.Api"
RUN dotnet publish "Karpaty.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Етап 2: Запуск (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Karpaty.Api.dll"]