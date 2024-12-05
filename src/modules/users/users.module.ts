import { Module } from "@nestjs/common";
import { UsersService } from "./users.service";
import { UsersController } from "./users.controller";
import { PrismaService } from "src/infrastructure/database/prisma/PrismaService";
import { RoleModule } from "../role/role.module";

@Module({
  imports: [RoleModule],
  controllers: [UsersController],
  providers: [UsersService, PrismaService],
})
export class UsersModule {}
