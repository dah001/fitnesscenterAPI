FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FitnessCenter.API/FitnessCenter.API.csproj",           "FitnessCenter.API/"]
COPY ["FitnessCenter.Core/FitnessCenter.Core.csproj",         "FitnessCenter.Core/"]
COPY ["FitnessCenter.Infrastructure/FitnessCenter.Infrastructure.csproj", "FitnessCenter.Infrastructure/"]

RUN dotnet restore "FitnessCenter.API/FitnessCenter.API.csproj"

COPY . .
WORKDIR "/src/FitnessCenter.API"
RUN dotnet build "FitnessCenter.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FitnessCenter.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FitnessCenter.API.dll"]
