# =============================
# Stage 1: Composer (dependencias)
# =============================
FROM composer:2 AS vendor
WORKDIR /app

COPY composer.json composer.lock ./
RUN composer install --no-dev --no-interaction --prefer-dist --no-scripts
COPY . .
RUN composer dump-autoload --optimize

# =============================
# Stage 2: Runtime (PHP CLI)
# =============================
FROM php:8.3-cli

# Instalación limpia con soporte SQL Server
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        gnupg \
        unixodbc-dev \
        git \
        unzip \
    # ✅ Forma moderna de agregar la clave Microsoft en Debian 11/12
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/11/prod bullseye main" > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 \
    && docker-php-ext-install pdo \
    && pecl install sqlsrv pdo_sqlsrv \
    && docker-php-ext-enable sqlsrv pdo_sqlsrv \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=vendor /app /app

# Variables por defecto
ENV APP_ENV=production \
    APP_DEBUG=false \
    APP_URL=http://localhost:8085 \
    DB_CONNECTION=sqlsrv \
    DB_HOST=tournaments-service \
    DB_PORT=1433 \
    DB_DATABASE=desaweb \
    DB_USERNAME=sa \
    DB_PASSWORD=Str0ngP@ssw0rd2025! \
    FRONT_ORIGIN=http://localhost:4200

RUN mkdir -p storage bootstrap/cache \
    && chown -R www-data:www-data storage bootstrap/cache \
    && chmod -R 775 storage bootstrap/cache

EXPOSE 8085

CMD sh -lc "\
  [ -f .env ] || cp .env.example .env; \
  grep -q '^APP_KEY=' .env && grep -q 'APP_KEY=base64:' .env || php artisan key:generate --force; \
  php artisan config:cache; \
  php artisan migrate --force || true; \
  php -d variables_order=EGPCS artisan serve --host=0.0.0.0 --port=8085 \
"
