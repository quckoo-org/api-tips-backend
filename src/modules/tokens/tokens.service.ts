import { Injectable } from "@nestjs/common";
import { JwtService } from "@nestjs/jwt";

@Injectable()
export class TokensService {
  private readonly refreshTokens = new Set<string>();

  constructor(private readonly jwtService: JwtService) {}

  async generateRefreshToken(userId: number): Promise<string> {
    const refreshToken = this.jwtService.sign(
      { sub: userId },
      { secret: process.env.JWT_REFRESH_SECRET, expiresIn: "7d" },
    );
    this.refreshTokens.add(refreshToken);
    return refreshToken;
  }

  async invalidateRefreshToken(refreshToken: string): Promise<void> {
    this.refreshTokens.delete(refreshToken);
  }

  async validateRefreshToken(refreshToken: string): Promise<any> {
    if (!this.refreshTokens.has(refreshToken)) {
      return null;
    }
    try {
      return this.jwtService.verify(refreshToken, {
        secret: process.env.JWT_REFRESH_SECRET,
      });
    } catch (err) {
      return null;
    }
  }
}
