FROM node:22

RUN corepack enable && corepack prepare yarn@4.5.1 --activate

WORKDIR /app

COPY package.json yarn.lock ./

RUN yarn install --frozen-lockfile

COPY . .

RUN yarn build

CMD ["yarn", "start:prod"]

EXPOSE 3000