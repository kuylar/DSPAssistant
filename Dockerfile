FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DSPAssistant/DSPAssistant.csproj", "DSPAssistant/"]
RUN dotnet restore "DSPAssistant/DSPAssistant.csproj"
COPY . .
WORKDIR "/src/DSPAssistant"
RUN dotnet build "DSPAssistant.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DSPAssistant.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DSPAssistant.dll"]
