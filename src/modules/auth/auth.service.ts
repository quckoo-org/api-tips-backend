// auth/auth.service.ts
import { Injectable, UnauthorizedException } from "@nestjs/common";
import { JwtService } from "@nestjs/jwt";
import * as bcrypt from "bcrypt";
import * as crypto from "crypto";
import { PrismaService } from "../../infrastructure/database/prisma/prisma.service";
import { EmailService } from "../email/email.service";

@Injectable()
export class AuthService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly jwtService: JwtService,
    private readonly verificationEmailService: EmailService,
  ) {}

  async register(
    email: string,
    password: string,
    name?: string,
    lastname?: string,
  ) {
    const existingUser = await this.prisma.user.findUnique({
      where: { email },
    });

    await this.verificationEmailService.sendVerificationEmail(
      "gmd29999@yandex.ru",
      "verificationToken",
    );

    if (existingUser) {
      throw new UnauthorizedException("Email already in use");
    }

    const hashedPassword = await bcrypt.hash(password, 10);

    const user = await this.prisma.user.create({
      data: { email, password: hashedPassword, name, lastname },
    });

    // Генерация токена подтверждения
    const verificationToken = this.jwtService.sign(
      { sub: user.id, email },
      {
        secret: process.env.JWT_SECRET || "verifySecret",
        expiresIn: "1h", // токен будет действителен в течение 1 часа
      },
    );

    // Отправка email с подтверждением
    await this.verificationEmailService.sendVerificationEmail(
      user.email,
      verificationToken,
    );

    return user;
  }

  async verifyEmailToken(token: string) {
    try {
      const payload = this.jwtService.verify(token, {
        secret: process.env.JWT_SECRET || "verifySecret",
      });
      await this.prisma.user.update({
        where: { id: payload.sub },
        data: { verifiedTimestamp: new Date() },
      });
      return { message: "Email verified successfully" };
    } catch (err) {
      throw new UnauthorizedException("Invalid verification token");
    }
  }

  async validateUser(email: string, password: string) {
    const user = await this.prisma.user.findUnique({ where: { email } });
    if (!user) {
      throw new UnauthorizedException("Invalid credentials");
    }

    const isPasswordValid = await bcrypt.compare(password, user.password);
    if (!isPasswordValid) {
      throw new UnauthorizedException("Invalid credentials");
    }

    return user;
  }

  generateAccessToken(userId: number, email: string): string {
    const payload = { sub: userId, email };
    return this.jwtService.sign(payload, {
      secret: process.env.JWT_SECRET || "accessSecret",
      expiresIn: "15m",
    });
  }

  generateRefreshToken(userId: number, email: string): string {
    const payload = { sub: userId, email };
    return this.jwtService.sign(payload, {
      secret: process.env.JWT_SECRET || "refreshSecret",
      expiresIn: "7d",
    });
  }

  verifyRefreshToken(token: string): any {
    try {
      return this.jwtService.verify(token, {
        secret: process.env.JWT_SECRET || "refreshSecret",
      });
    } catch (err) {
      throw new UnauthorizedException("Invalid refresh token");
    }
  }
}
