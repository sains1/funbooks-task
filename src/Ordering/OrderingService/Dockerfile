#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Ordering/OrderingService/OrderingService.csproj", "src/Ordering/OrderingService/"]
COPY ["src/Ordering/OrderingService.Contracts/OrderingService.Contracts.csproj", "src/Ordering/OrderingService.Contracts/"]
COPY ["src/Shared/ServiceDefaults/ServiceDefaults.csproj", "src/Shared/ServiceDefaults/"]
COPY ["src/Shared/SharedKernel/SharedKernel.csproj", "src/Shared/SharedKernel/"]
RUN dotnet restore "./src/Ordering/OrderingService/OrderingService.csproj"
COPY . .
WORKDIR "/src/src/Ordering/OrderingService"
RUN dotnet build "./OrderingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrderingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderingService.dll"]