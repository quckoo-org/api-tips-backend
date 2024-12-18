import { PrismaClient, Tariff } from "@prisma/client";
import { faker } from "@faker-js/faker";

const prisma = new PrismaClient();

async function main() {
  await prisma.tariff.deleteMany({}); // use with caution.

  const amountOfTariffs = 50;

  const tariffs: Tariff[] = [];

  for (let i = 0; i < amountOfTariffs; i++) {
    const user: Tariff = {
      id: i,
      name: faker.commerce.productName(),
      freeRequests: faker.number.int({ max: 100 }),
      paidRequests: faker.number.int({ min: 200, max: 10000 }),
      totalCost: faker.number.float({ min: 10, max: 1000 }),
      hidden: faker.datatype.boolean(),
      dateStart: faker.date.past(),
      dateExpiration: faker.date.recent(),
    };

    tariffs.push(user);
  }

  const addTariffs = async () =>
    await prisma.tariff.createMany({ data: tariffs });

  addTariffs();
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
