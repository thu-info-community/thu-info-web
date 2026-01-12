FROM bitnami/dotnet-sdk:10 AS build-env
WORKDIR /build
COPY . ./
RUN dotnet publish -c Release -o out

FROM bitnami/aspnet-core:10
WORKDIR /app
COPY --from=build-env /build/out .
ENTRYPOINT [ "dotnet", "ThuInfoWeb.dll"]
