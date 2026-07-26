FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR "/InfotecsTestTask"
COPY ["InfotecsTestTask.Web/InfotecsTestTask.Web.csproj", "InfotecsTestTask/InfotecsTestTask.Web/"]
RUN dotnet restore "InfotecsTestTask/InfotecsTestTask.Web/InfotecsTestTask.Web.csproj"

COPY . .
WORKDIR "/InfotecsTestTask"
RUN dotnet build "InfotecsTestTask.Web/InfotecsTestTask.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InfotecsTestTask.Web/InfotecsTestTask.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InfotecsTestTask.Web.dll"]