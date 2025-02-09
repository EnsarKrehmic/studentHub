FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StudentHub.csproj", "."]
RUN dotnet restore "./StudentHub.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StudentHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentHub.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentHub.dll"]
ENV ASPNETCORE_URLS=http://*:80