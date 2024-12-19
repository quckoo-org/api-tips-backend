import { PrismaClient, User } from "@prisma/client";
import { faker } from "@faker-js/faker";

const prisma = new PrismaClient();

async function main() {
  await prisma.user.deleteMany({}); // use with caution.

  const amountOfUsers = 50;

  const users: User[] = [];

  for (let i = 0; i < amountOfUsers; i++) {
    const user: User = {
      id: i,
      email: faker.internet.email(),
      password: faker.internet.password(),
      createdAt: faker.date.past(),
      name: faker.person.firstName(),
      lastname: faker.person.lastName(),
      blockedTimestamp: faker.date.past(),
      deletedTimestamp: faker.date.past(),
      verifiedTimestamp: faker.date.past(),
      countryCode: faker.location.countryCode("alpha-3"),
    };

    users.push(user);
  }

  const addUsers = async () => await prisma.user.createMany({ data: users });

  addUsers();
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
