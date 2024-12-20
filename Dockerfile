FROM node:22.11.0-alpine3.20 AS builder
WORKDIR /app

RUN corepack enable
RUN corepack prepare yarn@4.5.3 --activate

COPY .yarnrc.yml ./
COPY package.json yarn.lock ./
COPY src/infrastructure/database/prisma src/infrastructure/database/prisma

RUN yarn install --immutable --inline-builds
RUN yarn prisma:generate

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
EXPOSE 3001

CMD ["node", "dist/main"]