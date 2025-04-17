# HackerNews API

This ASP.NET Core Web API fetches the top 200 stories from Hacker News and serves them to the frontend.

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)

## How to Run
1. Open PowerShell.
2. Navigate to this folder (/HackerNews.API).
3. Run the script: HackerNewsApiRun.ps1

## Powershell
./HackerNewsApiRun.ps1

## Troubleshooting
If you see a script execution error, run this in PowerShell as administrator:
Run this command : Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

**Note**: To run API on Swagger UI go to (Make sure above poweshell script is running) : http://localhost:5000/swagger/index.html
