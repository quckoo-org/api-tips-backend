// auth/auth.controller.ts
import {
  Controller,
  Post,
  Body,
  Res,
  HttpCode,
  Get,
  Query,
  UnauthorizedException,
} from "@nestjs/common";
import { AuthService } from "./auth.service";
import { Response } from "express";
import { RegisterDto } from "./dto/register.dto";

@Controller("auth")
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Post("register")
  async register(@Body() body: RegisterDto) {
    return this.authService.register(body);
  }

  @Get("verify-email")
  async verifyEmail(@Query("token") token: string, @Res() response: Response) {
    try {
      console.log("token", token);
      await this.authService.verifyEmailToken(token);
      return response
        .status(200)
        .json({ status: "success", message: "Email verified successfully" });
    } catch (error) {
      return response
        .status(400)
        .json({ status: "error", message: "Verification failed" });
    }
  }

  @Post("login")
  @HttpCode(200)
  async login(
    @Body() body: { email: string; password: string },
    @Res({ passthrough: true }) response: Response,
  ) {
    const user = await this.authService.validateUser(body.email, body.password);
    if (!user.verifiedTimestamp) {
      throw new UnauthorizedException("Your email is not verified");
    }
    const accessToken = this.authService.generateAccessToken(
      user.id,
      user.email,
    );
    const refreshToken = this.authService.generateRefreshToken(
      user.id,
      user.email,
    );

    response.cookie("refreshToken", refreshToken, {
      httpOnly: true,
      secure: true,
      sameSite: "strict",
      maxAge: 7 * 24 * 60 * 60 * 1000, // 7 days
    });
    // Устанавливаем токен в cookie
    response.cookie("accessToken", accessToken, {
      secure: process.env.NODE_ENV === "production",
      maxAge: 15 * 60 * 1000,
    });
    return { user };
  }

  @Post("refresh")
  async refresh(
    @Body() body: { token: string },
    @Res({ passthrough: true }) response: Response,
  ) {
    const refreshToken = body.token;

    if (!refreshToken) {
      throw new UnauthorizedException("Refresh token missing");
    }

    const payload = this.authService.verifyRefreshToken(refreshToken);

    const newAccessToken = this.authService.generateAccessToken(
      payload.sub,
      payload.email,
    );
    const newRefreshToken = this.authService.generateRefreshToken(
      payload.sub,
      payload.email,
    );

    response.cookie("refreshToken", newRefreshToken, {
      httpOnly: true,
      secure: true,
      sameSite: "strict",
      maxAge: 7 * 24 * 60 * 60 * 1000,
    });

    return { accessToken: newAccessToken };
  }

  @Post("logout")
  @HttpCode(200)
  logout(@Res({ passthrough: true }) response: Response) {
    response.clearCookie("refreshToken");
    return { message: "Logged out" };
  }
}
