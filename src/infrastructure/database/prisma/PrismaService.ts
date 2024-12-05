import { Injectable, OnModuleInit, Logger } from "@nestjs/common";
import { PrismaClient } from "@prisma/client";

@Injectable()
export class PrismaService extends PrismaClient implements OnModuleInit {
  private readonly logger = new Logger(PrismaService.name);

  constructor() {
    const host = process.env.POSTGRES_HOST;
    const port = process.env.POSTGRES_PORT;
    const database = process.env.POSTGRES_DB;
    const user = process.env.POSTGRES_USER;
    const password = process.env.POSTGRES_PASSWORD;
    const schema = process.env.POSTGRES_SCHEMA;

    process.env.DATABASE_URL = `postgresql://${user}:${password}@${host}:${port}/${database}?schema=${schema}`;

    super();
  }

  async onModuleInit(): Promise<void> {
    try {
      await this.$connect();
      this.logger.log("Successfully connected to the database");
    } catch (error) {
      this.logger.error("Failed to connect to the database", error);
      throw error;
    }
  }
}
