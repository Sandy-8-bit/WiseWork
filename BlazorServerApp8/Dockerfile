FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BlazorServerApp8/BlazorServerApp8.csproj", "BlazorServerApp8/"]
RUN dotnet restore "BlazorServerApp8/BlazorServerApp8.csproj"
COPY . .
WORKDIR "/src/BlazorServerApp8"
RUN dotnet build "BlazorServerApp8.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorServerApp8.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorServerApp8.dll"]