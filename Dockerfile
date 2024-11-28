FROM node:22.11.0-alpine3.20 AS builder
WORKDIR /app

ARG POSTGRES_HOST=localhost
ENV POSTGRES_HOST=$POSTGRES_HOST

ARG POSTGRES_PORT=5432
ENV POSTGRES_PORT=$POSTGRES_PORT

ARG POSTGRES_DATABASE=postgres
ENV POSTGRES_DATABASE=$POSTGRES_DATABASE

ARG POSTGRES_PASSWORD=password
ENV POSTGRES_PASSWORD=$POSTGRES_PASSWORD

RUN corepack enable
RUN corepack prepare yarn@4.5.1 --activate

COPY .yarnrc.yml ./
COPY package.json yarn.lock ./
COPY src/infrastructure/database/prisma ./prisma/

RUN yarn install --immutable --inline-builds
RUN npx prisma generate

COPY . .
RUN yarn build:proto:docker
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