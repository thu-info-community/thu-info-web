FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build-env
WORKDIR /build
COPY . ./
RUN dotnet publish ThuInfoWeb/ThuInfoWeb.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble
WORKDIR /app
COPY --from=build-env /build/out .
ENTRYPOINT [ "dotnet", "ThuInfoWeb.dll"]
