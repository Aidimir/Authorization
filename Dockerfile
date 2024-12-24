FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV DOTNET_URLS=http://+:5000
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "Api/Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api/Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
