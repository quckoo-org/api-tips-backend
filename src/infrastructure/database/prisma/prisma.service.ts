import { Injectable, OnModuleInit, Logger } from "@nestjs/common";
import { PrismaClient } from "@prisma/client";
import * as process from "process";

@Injectable()
export class PrismaService extends PrismaClient implements OnModuleInit {
  private readonly logger = new Logger(PrismaService.name);

  constructor() {
    super();
  }

  async onModuleInit(): Promise<void> {
    try {
      console.log(process.env.DATABASE_URL);
      await this.$connect();
      this.logger.log("Successfully connected to the database");
    } catch (error) {
      this.logger.error("Failed to connect to the database", error);
      throw error;
    }
  }
}
