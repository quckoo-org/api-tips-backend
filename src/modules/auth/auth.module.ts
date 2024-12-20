import { Module } from "@nestjs/common";
import { AuthService } from "./auth.service";
import { AuthController } from "./auth.controller";
import { JwtModule } from "@nestjs/jwt";
import { PassportModule } from "@nestjs/passport";
import { JwtStrategy } from "../tokens/strategies/jwt.strategy";
import { PrismaService } from "../../infrastructure/database/prisma/prisma.service";
import { EmailService } from "../email/email.service";

@Module({
  imports: [
    PassportModule,
    JwtModule.register({
      secret: process.env.JWT_SECRET || "secretKey",
      signOptions: { expiresIn: "15m" }, // Access токен на 15 минут
    }),
  ],
  controllers: [AuthController],
  providers: [AuthService, JwtStrategy, PrismaService, EmailService],
  exports: [AuthService],
})
export class AuthModule {}
