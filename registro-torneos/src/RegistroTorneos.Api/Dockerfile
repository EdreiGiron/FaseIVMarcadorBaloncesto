# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./RegistroTorneos.Api/RegistroTorneos.Api.csproj
RUN dotnet publish ./RegistroTorneos.Api/RegistroTorneos.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5057
EXPOSE 5057
ENTRYPOINT ["dotnet","RegistroTorneos.Api.dll"]
