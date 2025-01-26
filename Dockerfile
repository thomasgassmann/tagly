# for Api
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY . /source
RUN dotnet restore
RUN dotnet publish -c release --property:PublishDir=/app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Tagly.Api.dll"]
