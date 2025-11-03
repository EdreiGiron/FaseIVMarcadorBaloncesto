# ------------ Build stage ------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./scr/AuthService.Api.csproj ./AuthService.Api.csproj
RUN dotnet restore ./AuthService.Api.csproj

COPY ./scr/ ./
RUN dotnet publish ./AuthService.Api.csproj -c Release -o /app /p:UseAppHost=false

# ------------ Runtime stage ------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","AuthService.Api.dll"]
