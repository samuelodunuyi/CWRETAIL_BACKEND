# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore dependencies
COPY ["CWRETAIL_BACKEND.csproj", "./"]
RUN dotnet restore "CWRETAIL_BACKEND.csproj"

# copy everything else and build
COPY . .
RUN dotnet publish "CWRETAIL_BACKEND.csproj" -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "CWRETAIL_BACKEND.dll"]
