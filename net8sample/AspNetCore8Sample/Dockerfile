FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AspNetCore8Sample/AspNetCore8Sample.csproj", "AspNetCore8Sample/"]
RUN dotnet restore "AspNetCore8Sample/AspNetCore8Sample.csproj"
COPY . .
WORKDIR "/src/AspNetCore8Sample"
RUN dotnet publish "AspNetCore8Sample.csproj" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore8Sample.dll"]
USER app