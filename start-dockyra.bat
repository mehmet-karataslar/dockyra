@echo off
taskkill /IM Dockyra.Desktop.exe /F >nul 2>&1
start "" "%~dp0src\Dockyra.Desktop\bin\Debug\net10.0-windows\Dockyra.Desktop.exe"
