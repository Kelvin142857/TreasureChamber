# ============================================================
# TreasureChamber 珍宝展厅 · 产品管理系统（Vue3 + PrimeVue SPA）
# 三阶段构建：node 构建前端 → SDK 编译发布后端 → aspnet 运行时
# 构建: docker build -t treasurechamber:latest .
# 运行: docker run -d -p 5000:5000 -v treasurechamber-data:/app/App_Data --name treasurechamber treasurechamber:latest
# ============================================================

# ---------- 阶段一：前端 Vue + PrimeVue ----------
FROM node:24-alpine AS frontend
WORKDIR /src/client
COPY src/TreasureChamber.WebApp/ClientApp/package.json src/TreasureChamber.WebApp/ClientApp/package-lock.json ./
RUN npm ci --no-audit --no-fund && (npm install-scripts approve esbuild 2>/dev/null || true) && npm rebuild esbuild
COPY src/TreasureChamber.WebApp/ClientApp/ ./
# Vite 输出到 ../wwwroot（相对 ClientApp）=> /src/wwwroot
RUN npm run build

# ---------- 阶段二：后端编译发布 ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/TreasureChamber.Core/TreasureChamber.Core.csproj src/TreasureChamber.Core/
COPY src/TreasureChamber.Data/TreasureChamber.Data.csproj src/TreasureChamber.Data/
COPY src/TreasureChamber.Application/TreasureChamber.Application.csproj src/TreasureChamber.Application/
COPY src/TreasureChamber.WebApp/TreasureChamber.WebApp.csproj src/TreasureChamber.WebApp/
COPY .docker/nuget.config .docker/nuget.config

RUN dotnet restore src/TreasureChamber.WebApp/TreasureChamber.WebApp.csproj --configfile .docker/nuget.config

COPY src/ src/
WORKDIR /src/src/TreasureChamber.WebApp
RUN dotnet publish TreasureChamber.WebApp.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

# ---------- 阶段三：运行时 ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5000

COPY --chown=app:app --from=build /app/publish .
# 覆盖为容器内重新构建的前端产物
COPY --chown=app:app --from=frontend /src/wwwroot ./wwwroot
COPY docker/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
