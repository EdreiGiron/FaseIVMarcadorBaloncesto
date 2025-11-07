
# ===== build =====
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
# build prod (ajusta si usas Angular <=15)
RUN npm run build --configuration=production

# ===== nginx ===== Stage 2: Serve app with Nginx
FROM nginx:alpine
# copia la app Angular
COPY --from=build /app/dist/marcador-frontend/browser /usr/share/nginx/html
# nginx.conf propio (con proxy /api → http://api:5051)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
