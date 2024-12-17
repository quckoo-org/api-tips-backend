import {
  Controller,
  Post,
  Body,
  Res,
  Req,
  HttpCode,
  ValidationPipe,
} from "@nestjs/common";
import { AuthService } from "./auth.service";
import { Response, Request } from "express";
import { RegisterDto } from "./dto/register.dto";
import { LoginDto } from "./dto/login.dto";

@Controller("auth")
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Post("register")
  async register(
    @Body(new ValidationPipe({ whitelist: true })) body: RegisterDto,
  ) {
    return this.authService.register(body);
  }

  @Post("login")
  @HttpCode(200)
  async login(
    @Body(new ValidationPipe({ whitelist: true })) body: LoginDto,
    @Res({ passthrough: true }) response: Response,
  ) {
    const user = await this.authService.validateUser(body.email, body.password);
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

    return { accessToken };
  }

  @Post("refresh")
  async refresh(
    @Req() request: Request,
    @Res({ passthrough: true }) response: Response,
  ) {
    const refreshToken = request.cookies["refreshToken"];
    if (!refreshToken) {
      throw new Error("Refresh token not found"); // Можно использовать кастомные исключения
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
      maxAge: 7 * 24 * 60 * 60 * 1000, // 7 days
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
