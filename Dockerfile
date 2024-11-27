FROM node:22.11.0-alpine3.20 AS builder
WORKDIR /app

ARG POSTGRES_USER

ARG POSTGRES_PASSWORD

ARG POSTGRES_DB

ARG DB_PORT

ARG DB_HOST

ARG DB_SCHEMA

ENV DATABASE_URL="postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${DB_HOST}:${DB_PORT}/${POSTGRES_DB}?schema=${DB_SCHEMA}"

RUN corepack enable
RUN corepack prepare yarn@4.5.1 --activate

COPY .yarnrc.yml ./
COPY package.json yarn.lock ./
COPY src/infrastructure/database/prisma ./prisma/

RUN yarn install --immutable --inline-builds
RUN prisma generate

COPY . .
RUN yarn build:proto
RUN yarn build

FROM node:22.11.0-alpine3.20 AS runner
WORKDIR /app

RUN corepack enable
RUN corepack prepare yarn@4.5.1 --activate

COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules
COPY package.json yarn.lock ./

EXPOSE 3000

CMD ["node", "dist/main"]