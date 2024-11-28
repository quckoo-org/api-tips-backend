import { Module } from '@nestjs/common';
import { TestService } from './test.service';
import { TestController } from './test.controller';
import { PrismaService } from '../../infrastructure/database/prisma/PrismaService';

@Module({
  providers: [TestService, PrismaService],
  controllers: [TestController],
})
export class TestModule {}
