import { PrismaClient, User } from "@prisma/client";
import { faker } from "@faker-js/faker";

const prisma = new PrismaClient();

async function main() {
  await prisma.user.deleteMany({}); // use with caution.

  const amountOfUsers = 51;

  const users: Omit<User, "id">[] = [];

  // TODO: Разобраться с индексами в БД
  for (let i = 1; i < amountOfUsers; i++) {
    const user: Omit<User, "id"> = {
      email: faker.internet.email(),
      password: faker.internet.password(),
      createdAt: faker.date.past(),
      firstName: faker.person.firstName(),
      lastName: faker.person.lastName(),
      blockedAt: faker.date.past(),
      deletedAt: faker.date.past(),
      verifiedAt: faker.date.past(),
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
