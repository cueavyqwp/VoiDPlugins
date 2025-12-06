@echo off
cd /d %~dp0
dotnet publish ./src/WindowsInk --configuration Release --framework net8.0 -o ./build/WindowsInk