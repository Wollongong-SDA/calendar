# Using .NET 9 distroless for now, may want to switch to .NET LTS at some point
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-noble AS base
RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | sh
RUN apt-get install -y nodejs
RUN corepack enable
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"

FROM base AS build
ARG TARGETARCH
WORKDIR /source

COPY --link standalone-ui/*.esproj /standalone-ui/
COPY --link calendar-aggregator/*.csproj /calendar-aggregator/
RUN dotnet restore -a $TARGETARCH /standalone-ui/standalone-ui.esproj
RUN dotnet restore -a $TARGETARCH /calendar-aggregator/CalendarAggregator.csproj

COPY --link standalone-ui/. /standalone-ui/
COPY --link calendar-aggregator/. /calendar-aggregator/
RUN pnpm install --prefix /standalone-ui --frozen-lockfile
RUN dotnet publish -a $TARGETARCH --no-restore -o /app /calendar-aggregator/CalendarAggregator.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled
EXPOSE 8080
WORKDIR /app
COPY --link --from=build /app ./calendar-aggregator
ENTRYPOINT ["./calendar-aggregator"]
