# =============================
# Stage 1: Composer (dependencias)
# =============================
FROM composer:2 AS vendor
WORKDIR /app

# Copiamos definiciones y resolvemos dependencias sin dev
COPY composer.json composer.lock ./
RUN composer install --no-dev --no-interaction --prefer-dist --no-scripts

# Copiamos el resto del código y optimizamos autoload
COPY . .
RUN composer dump-autoload --optimize

# =============================
# Stage 2: Runtime (PHP CLI)
# =============================
FROM php:8.3-cli

# Paquetes necesarios y extensión pdo_mysql
RUN apt-get update \
    && apt-get install -y --no-install-recommends default-mysql-client git unzip \
    && docker-php-ext-install pdo pdo_mysql \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copiamos la app con vendor ya resuelto
COPY --from=vendor /app /app

# Variables por defecto
ENV APP_ENV=production \
    APP_DEBUG=false \
    APP_URL=http://localhost:8082 \
    DB_CONNECTION=mysql \
    DB_HOST=players-db \
    DB_PORT=3306 \
    DB_DATABASE=players \
    DB_USERNAME=players \
    DB_PASSWORD=players123 \
    FRONT_ORIGIN=http://localhost:4200

# Permisos de escritura
RUN mkdir -p storage bootstrap/cache \
    && chown -R www-data:www-data storage bootstrap/cache \
    && chmod -R 775 storage bootstrap/cache

EXPOSE 8082

# Comando de inicio
CMD sh -lc "\
  [ -f .env ] || cp .env.example .env; \
  grep -q '^APP_KEY=' .env && grep -q 'APP_KEY=base64:' .env || php artisan key:generate --force; \
  php artisan config:cache; \
  php artisan migrate --force || true; \
  php -d variables_order=EGPCS artisan serve --host=0.0.0.0 --port=8082 \
"