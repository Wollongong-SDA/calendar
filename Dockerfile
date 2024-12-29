# Using .NET 9 distroless for now, may want to switch to .NET LTS at some point
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG TARGETARCH
WORKDIR /source

COPY --link calendar-aggregator/*.csproj .
RUN dotnet restore -a $TARGETARCH

COPY --link calendar-aggregator/. .
RUN dotnet publish -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled
EXPOSE 8080
WORKDIR /app
COPY --link --from=build /app .
ENTRYPOINT ["./calendar-aggregator"]
