FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /app

COPY src/* .

RUN dotnet restore
RUN dotnet publish -c Release -o output --no-restore

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS run-env
WORKDIR /app

COPY --from=build-env /app/output .
COPY --from=build-env /app/config.hjson .
ENTRYPOINT ["dotnet", "ISISLab.HelpDesk.dll"]