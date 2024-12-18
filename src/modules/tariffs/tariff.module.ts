import { Module } from "@nestjs/common";
import { TariffController } from "./tariff.controller";
import { TariffService } from "./tariff.service";
import { PrismaService } from "../../infrastructure/database/prisma/PrismaService";

@Module({
  controllers: [TariffController],
  providers: [TariffService, PrismaService],
})
export class TariffModule {}
