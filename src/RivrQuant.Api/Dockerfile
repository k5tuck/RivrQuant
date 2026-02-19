FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RivrQuant.Domain/RivrQuant.Domain.csproj", "RivrQuant.Domain/"]
COPY ["RivrQuant.Infrastructure/RivrQuant.Infrastructure.csproj", "RivrQuant.Infrastructure/"]
COPY ["RivrQuant.Application/RivrQuant.Application.csproj", "RivrQuant.Application/"]
COPY ["RivrQuant.Api/RivrQuant.Api.csproj", "RivrQuant.Api/"]
RUN dotnet restore "RivrQuant.Api/RivrQuant.Api.csproj"
COPY . .
WORKDIR "/src/RivrQuant.Api"
RUN dotnet build "RivrQuant.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RivrQuant.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "RivrQuant.Api.dll"]
