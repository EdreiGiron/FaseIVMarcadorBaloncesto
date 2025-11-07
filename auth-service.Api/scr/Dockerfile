# ------------ Build stage ------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ./scr/AuthService.Api.csproj ./AuthService.Api.csproj
RUN dotnet restore ./AuthService.Api.csproj

COPY ./scr/ ./
RUN dotnet publish ./AuthService.Api.csproj -c Release -o /app /p:UseAppHost=false

# ------------ Runtime stage ------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install EF Core tools in runtime
RUN apt-get update && apt-get install -y curl
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --install-dir /usr/share/dotnet
RUN rm -f /usr/bin/dotnet && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY --from=build /app .
COPY --from=build /src .

# Create entrypoint script
RUN echo '#!/bin/bash\n\
echo "Waiting for database..."\n\
sleep 15\n\
echo "Running migrations..."\n\
dotnet ef database update --no-build || echo "Migration failed, continuing..."\n\
echo "Starting application..."\n\
dotnet AuthService.Api.dll' > /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["/app/entrypoint.sh"]
