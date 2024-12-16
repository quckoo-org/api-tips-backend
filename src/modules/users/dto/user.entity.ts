import type { User } from "@prisma/client";

export class UserEntity implements User {
  id: number;
  email: string;
  password: string;
  createdAt: Date;
  name: string;
  lastname: string;
  blockedTimestamp: Date | null;
  deletedTimestamp: Date | null;
  verifiedTimestamp: Date | null;
  countryCode: string;
}
