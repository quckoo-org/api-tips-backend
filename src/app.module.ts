import { Module } from "@nestjs/common";
import { AppController } from "./app.controller";
import { AppService } from "./app.service";
import { TestModule } from "./modules/test/test.module";
import { ConfigModule } from "@nestjs/config";
import { HealthController } from "./modules/health/health.controller";
import { UsersModule } from "./modules/users/users.module";
import { RoleModule } from "./modules/role/role.module";
import { AutomapperModule } from "@automapper/nestjs";
import { classes } from "@automapper/classes";

@Module({
  imports: [
    RoleModule,
    UsersModule,
    TestModule,
    ConfigModule.forRoot({
      isGlobal: true, // Делаем ConfigModule доступным во всем приложении
      envFilePath: ".env", // Путь к вашему .env файлу (по умолчанию `.env`)
    }),
    AutomapperModule.forRoot({
      strategyInitializer: classes(),
    }),
  ],
  controllers: [AppController, HealthController],
  providers: [AppService],
})
export class AppModule {}
