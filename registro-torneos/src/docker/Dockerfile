# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/ ./src/
WORKDIR /src/RegistroTorneos.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet","RegistroTorneos.Api.dll"]
