FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
# install dotnet tool
RUN dotnet tool install --global dotnet-dump
RUN dotnet tool install --global dotnet-gcdump
RUN dotnet tool install --global dotnet-counters

WORKDIR /src
COPY ["DiagnosticSample.csproj", "./"]
RUN dotnet restore "DiagnosticSample.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DiagnosticSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiagnosticSample.csproj" -c Release -o /app/publish

FROM base AS final

COPY --from=build-env /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiagnosticSample.dll"]
