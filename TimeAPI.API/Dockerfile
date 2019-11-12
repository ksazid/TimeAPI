FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["TimeAPI.API/TimeAPI.API.csproj", "TimeAPI.API/"]
COPY ["TimeAPI.Data/TimeAPI.Data.csproj", "TimeAPI.Data/"]
RUN dotnet restore "TimeAPI.API/TimeAPI.API.csproj"
COPY . .
WORKDIR "/src/TimeAPI.API"
RUN dotnet build "TimeAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeAPI.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeAPI.API.dll"]