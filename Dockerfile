FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /build
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /build/out .
ENTRYPOINT [ "dotnet", "ThuInfoWeb.dll"]
