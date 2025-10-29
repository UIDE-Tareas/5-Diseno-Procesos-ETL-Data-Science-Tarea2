@echo off
chcp 65001 >nul
echo ==============================
echo 🚀 Ejecutando procesos en paralelo
echo ==============================
start "WebApi" /MAX dotnet run --project "Tarea2.Ejercicio1.WebApi" --configuration Release
start "WebApp" /MAX dotnet run --project "Tarea2.Ejercicio1.WebApp" --configuration Release
start "ConsoleApp" /MAX dotnet run --project "Tarea2.Ejercicio2.ConsoleApp" --configuration Release
echo ==============================
echo ✅ Todos los procesos fueron iniciados.
echo ==============================
pause