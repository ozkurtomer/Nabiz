FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
# Demo hesap butonlarını canlıda kapatmak için: --build-arg DEMO_GIRIS=false
ARG DEMO_GIRIS=true
RUN sed -i "s/demoGiris: true/demoGiris: ${DEMO_GIRIS}/" src/environments/environment.ts
RUN npx ng build --configuration production

FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
EXPOSE 80
