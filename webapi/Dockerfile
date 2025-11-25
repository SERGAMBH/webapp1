FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder 
#EXPOSE 5000
#EXPOSE 5001
EXPOSE 80
EXPOSE 443

WORKDIR /Application

#ENV ASPNETCORE_ENVIRONMENT=Development

# Обновляем CA сертификаты
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

# Копируем только файлы проектов для восстановления пакетов
#COPY ["webapi.csproj", "nuget.config", "./"]
COPY ["webapi.csproj", "./"]

# Восстанавливаем пакеты из nuget-packages (если есть) и официальных источников
#COPY nuget-packages/ /nuget-packages/
RUN dotnet restore "webapi.csproj" 
#--source /nuget-packages --source https://api.nuget.org/v3/index.json

# Копируем остальные файлы проекта
COPY . ./

# Публикуем приложение
RUN dotnet publish -c Release -o output

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /Application
COPY --from=builder /Application/output .
#ENV ASPNETCORE_URLS="http://+:5000;https://+:5001"
#ENV ASPNETCORE_HTTPS_PORT=5001
#ENV ASPNETCORE_URLS=http://+:8080
#ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "webapi.dll"]