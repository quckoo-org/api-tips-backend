import { Module } from "@nestjs/common";
import { UsersService } from "./users.service";
import { UsersController } from "./users.controller";
import { PrismaService } from "src/infrastructure/database/prisma/prisma.service";
import { RoleModule } from "../role/role.module";
import { UserProfile } from "./users.mapper.profile";
import { AutomapperModule } from "@automapper/nestjs";

@Module({
  imports: [RoleModule, AutomapperModule],
  controllers: [UsersController],
  providers: [UsersService, PrismaService, UserProfile],
})
export class UsersModule {}
