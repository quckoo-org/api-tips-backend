import type { User } from "@prisma/client";

export class UserEntity implements User {
  id: number;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  countryCode: string | null;
  createdAt: Date;
  blockedAt: Date | null;
  deletedAt: Date | null;
  verifiedAt: Date | null;
}
