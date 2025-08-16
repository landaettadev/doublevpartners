# Script para ejecutar la API
Write-Host "Iniciando la API..." -ForegroundColor Green
Write-Host "Puerto HTTP: http://localhost:5000" -ForegroundColor Yellow
Write-Host "Puerto HTTPS: https://localhost:5001" -ForegroundColor Yellow
Write-Host "Swagger UI: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "Presiona Ctrl+C para detener" -ForegroundColor Red
Write-Host ""

cd src/Api
dotnet run
