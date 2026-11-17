FROM mcr.microsoft.com/dotnet/sdk:11.0-resolute AS build-env
WORKDIR /build
COPY . ./
RUN dotnet publish ThuInfoWeb/ThuInfoWeb.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:11.0-resolute
WORKDIR /app
COPY --from=build-env /build/out .
ENTRYPOINT [ "dotnet", "ThuInfoWeb.dll"]
