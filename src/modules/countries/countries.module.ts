import { Module } from "@nestjs/common";
import { CountriesService } from "./countries.service";
import { CountriesController } from "./countries.controller";
import { PrismaService } from "src/infrastructure/database/prisma/PrismaService";

@Module({
  controllers: [CountriesController],
  providers: [CountriesService, PrismaService],
})
export class CountriesModule {}
