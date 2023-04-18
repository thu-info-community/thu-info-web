FROM bitnami/dotnet-sdk:6 AS build-env
WORKDIR /build
COPY . ./
RUN dotnet publish -c Release -o out

FROM ubuntu/dotnet-aspnet:6.0-22.04_beta
WORKDIR /app
COPY --from=build-env /build/out .
ENTRYPOINT [ "dotnet", "ThuInfoWeb.dll", "--urls", "http://0.0.0.0:80"]
