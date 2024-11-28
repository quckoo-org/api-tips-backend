import { Injectable, OnModuleInit } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';

@Injectable()
export class PrismaService extends PrismaClient implements OnModuleInit {
  constructor() {
    const host = process.env.POSTGRES_HOST || 'localhost';
    const port = process.env.POSTGRES_PORT || '5432';
    const database = process.env.POSTGRES_DATABASE || 'postgres';
    const user = process.env.POSTGRES_USER || 'postgres';
    const password = process.env.POSTGRES_PASSWORD || 'password';
    const schema = process.env.POSTGRES_PASSWORD || 'public';

    process.env.DATABASE_URL = `postgresql://${user}:${password}@${host}:${port}/${database}?schema=${schema}`; // Устанавливаем в переменную окружения

    super();
  }

  async onModuleInit(): Promise<void> {
    await this.$connect();
  }
}
