#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.OfflineLoginRewardService/Tgs.OfflineLoginRewardService.csproj", "Tgs.OfflineLoginRewardService/"]
COPY ["OfflineLoginReward.Cache/OfflineLoginReward.Cache.csproj", "OfflineLoginReward.Cache/"]
COPY ["OfflineLoginReward.Core/OfflineLoginReward.Core.csproj", "OfflineLoginReward.Core/"]
COPY ["OfflineLoginReward.Repositories/OfflineLoginReward.Repositories.csproj", "OfflineLoginReward.Repositories/"]
RUN dotnet restore "Tgs.OfflineLoginRewardService/Tgs.OfflineLoginRewardService.csproj"
COPY . .
WORKDIR "/src/Tgs.OfflineLoginRewardService"
RUN dotnet build "Tgs.OfflineLoginRewardService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.OfflineLoginRewardService.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.OfflineLoginRewardService.dll"]