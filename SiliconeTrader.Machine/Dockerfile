#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SiliconeTrader.Machine/SiliconeTrader.Machine.csproj", "SiliconeTrader.Machine/"]
RUN dotnet restore "SiliconeTrader.Machine/SiliconeTrader.Machine.csproj"
COPY . .
WORKDIR "/src/SiliconeTrader.Machine"
RUN dotnet build "SiliconeTrader.Machine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SiliconeTrader.Machine.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SiliconeTrader.Machine.dll"]