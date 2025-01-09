FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.4-alpine3.19-amd64 AS base
ENV DOTNET_EnableDiagnostics=0

LABEL owner="Dev team"

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.204-alpine3.19-amd64 AS build

# Install glibc
ARG GLIBC_VER="2.35-r1"
ARG ALPINE_GLIBC_REPO="https://github.com/sgerrand/alpine-pkg-glibc/releases/download"
ARG SGERRAND_RSA_SHA256="823b54589c93b02497f1ba4dc622eaef9c813e6b0f0ebbb2f771e32adf9f4ef2"

RUN apk add --no-cache --virtual .build-deps curl binutils zstd gcompat && \
    curl -LfsS https://alpine-pkgs.sgerrand.com/sgerrand.rsa.pub -o /etc/apk/keys/sgerrand.rsa.pub && \
    echo "${SGERRAND_RSA_SHA256} */etc/apk/keys/sgerrand.rsa.pub" | sha256sum -c - && \
    curl -LfsS ${ALPINE_GLIBC_REPO}/${GLIBC_VER}/glibc-${GLIBC_VER}.apk > /tmp/glibc-${GLIBC_VER}.apk && \
    apk add --force-overwrite --no-cache /tmp/glibc-${GLIBC_VER}.apk && \
    apk del gcompat && \
    curl -LfsS ${ALPINE_GLIBC_REPO}/${GLIBC_VER}/glibc-bin-${GLIBC_VER}.apk > /tmp/glibc-bin-${GLIBC_VER}.apk && \
    apk add --no-cache /tmp/glibc-bin-${GLIBC_VER}.apk && \
    curl -Ls ${ALPINE_GLIBC_REPO}/${GLIBC_VER}/glibc-i18n-${GLIBC_VER}.apk > /tmp/glibc-i18n-${GLIBC_VER}.apk && \
    apk add --no-cache /tmp/glibc-i18n-${GLIBC_VER}.apk

WORKDIR /source

COPY ["src/ApiTips.Api/ApiTips.Api.csproj", "src/ApiTips.Api/"]
COPY ["src/ApiTips.Dal/ApiTips.Dal.csproj", "src/ApiTips.Dal/"]
RUN dotnet restore "./src/ApiTips.Api/ApiTips.Api.csproj"  -r linux-musl-x64
COPY . .
WORKDIR "/source/src/ApiTips.Api"

FROM build AS publish
RUN GRPC_TOOL_PLUGIN=/usr/glibc-compat dotnet publish "ApiTips.Api.csproj" \
    -c Release \
    -o /app/publish \
    -r linux-musl-x64 \
    --self-contained true \
    --no-restore \
    /p:PublishTrimmed=true \
    /p:LinkMode=Link \
    /p:PublishSingleFile=true \
    /p:EnableCompressionInSingleFile=true \
    /p:IncludeAllContentForSelfExtract=true

FROM base AS final

# Credentials
ARG POSTGRES_PASSWORD
ENV POSTGRES_PASSWORD $POSTGRES_PASSWORD

ARG REDIS_PASSWORD
ENV REDIS_PASSWORD $REDIS_PASSWORD

ARG JWT_SECRET_KEY
ENV JWT_SECRET_KEY $JWT_SECRET_KEY

COPY --from=publish /app/publish .
ENTRYPOINT ["./ApiTips.Api"]