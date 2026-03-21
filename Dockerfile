# ═══════════════════════════════════════════════════════════
# COWORK.ARMY — Multi-stage build (Node frontend + .NET backend)
# Single service: frontend → wwwroot, backend serves everything
# ═══════════════════════════════════════════════════════════

# ── Stage 1: Frontend build ──
FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY frontend/package.json frontend/package-lock.json* ./
RUN npm install --legacy-peer-deps
COPY frontend/ .
# Same origin — no API URL prefix needed
ENV VITE_API_URL=""
ENV VITE_SIGNALR_URL="/hub"
RUN npm run build

# ── Stage 2: Backend build ──
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY backend/src/CoworkArmy/ .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# ── Stage 3: Production runtime ──
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy .NET published output
COPY --from=backend-build /app .

# Copy frontend build to wwwroot (static files served by ASP.NET)
COPY --from=frontend-build /frontend/dist ./wwwroot

# Copy database schema (for reference/migrations)
COPY database/schema.sql ./schema.sql

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8888
EXPOSE 8888

ENTRYPOINT ["dotnet", "CoworkArmy.dll"]
