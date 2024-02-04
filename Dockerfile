FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CloudWeather.Temperature/CloudWeather.Temperature.csproj", "CloudWeather.Temperature/"]
COPY ["CloudWeather.Precipitation/CloudWeather.Precipitation.csproj", "CloudWeather.Precipitation/"]
COPY ["CloudWeather.Report/CloudWeather.Report.csproj", "CloudWeather.Report/"]
COPY ["CloudWeather.DataLoader/CloudWeather.DataLoader.csproj", "CloudWeather.DataLoader/"]
RUN dotnet restore "./CloudWeather.Temperature/./CloudWeather.Temperature.csproj"
RUN dotnet restore "./CloudWeather.Precipitation/./CloudWeather.Precipitation.csproj"
RUN dotnet restore "./CloudWeather.Report/./CloudWeather.Report.csproj"
RUN dotnet restore "./CloudWeather.DataLoader/./CloudWeather.DataLoader.csproj"

COPY . .
WORKDIR "/src/CloudWeather.Temperature"
RUN dotnet build "./CloudWeather.Temperature.csproj" -c $BUILD_CONFIGURATION -o /app/build01

COPY . .
WORKDIR "/src/CloudWeather.Precipitation"
RUN dotnet build "./CloudWeather.Precipitation.csproj" -c $BUILD_CONFIGURATION -o /app/build02

COPY . .
WORKDIR "/src/CloudWeather.Report"
RUN dotnet build "./CloudWeather.Report.csproj" -c $BUILD_CONFIGURATION -o /app/build03

COPY . .
WORKDIR "/src/CloudWeather.DataLoader"
RUN dotnet build "./CloudWeather.DataLoader.csproj" -c $BUILD_CONFIGURATION -o /app/build04


FROM build AS publish01
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudWeather.Temperature.csproj" -c $BUILD_CONFIGURATION -o /app/publish01 /p:UseAppHost=false

FROM build AS publish02
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudWeather.Precipitation.csproj" -c $BUILD_CONFIGURATION -o /app/publish02 /p:UseAppHost=false

FROM build AS publish03
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudWeather.Report.csproj" -c $BUILD_CONFIGURATION -o /app/publish03 /p:UseAppHost=false

FROM build AS publish04
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudWeather.DataLoader.csproj" -c $BUILD_CONFIGURATION -o /app/publish04 /p:UseAppHost=false

FROM base AS final01
WORKDIR /app
COPY --from=publish01 /app/publish01 .
EXPOSE 5000
ENTRYPOINT ["dotnet", "CloudWeather.Temperature.dll"]

FROM base AS final02
WORKDIR /app
COPY --from=publish02 /app/publish02 .
EXPOSE 5000
ENTRYPOINT ["dotnet", "CloudWeather.Precipitation.dll"]

FROM base AS final03
WORKDIR /app
COPY --from=publish03 /app/publish03 .
EXPOSE 5000
ENTRYPOINT ["dotnet", "CloudWeather.Report.dll"]

FROM base AS final04
WORKDIR /app
COPY --from=publish04 /app/publish04 .
EXPOSE 5000
ENTRYPOINT ["dotnet", "CloudWeather.DataLoader.dll"]

