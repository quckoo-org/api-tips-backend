import { Module } from "@nestjs/common";
import { AppController } from "./app.controller";
import { AppService } from "./app.service";
import { TestModule } from "./modules/test/test.module";
import { ConfigModule } from "@nestjs/config";
import { HealthController } from "./modules/health/health.controller";
import { AuthModule } from "./modules/auth/auth.module";

@Module({
  imports: [
    TestModule,
    AuthModule,
    ConfigModule.forRoot({
      isGlobal: true, // Делаем ConfigModule доступным во всем приложении
      envFilePath: ".env", // Путь к вашему .env файлу (по умолчанию `.env`)
    }),
  ],
  controllers: [AppController, HealthController],
  providers: [AppService],
})
export class AppModule {}
