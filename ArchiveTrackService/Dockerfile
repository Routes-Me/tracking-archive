#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ArchiveTrackService/ArchiveTrackService.csproj", "ArchiveTrackService/"]
RUN dotnet restore "ArchiveTrackService/ArchiveTrackService.csproj"
COPY . .
WORKDIR "/src/ArchiveTrackService"
RUN dotnet build "ArchiveTrackService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArchiveTrackService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArchiveTrackService.dll"]