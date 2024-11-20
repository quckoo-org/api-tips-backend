import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { TestModule } from './test/test.module';
import { ConfigModule } from '@nestjs/config';

@Module({
  imports: [
    TestModule,
    ConfigModule.forRoot({
      isGlobal: true, // Делаем ConfigModule доступным во всем приложении
      envFilePath: '.env', // Путь к вашему .env файлу (по умолчанию `.env`)
    }),
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}
