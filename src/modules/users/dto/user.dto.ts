import { Timestamp } from "src/proto/google/protobuf/timestamp";
import { User } from "src/proto/user/v1/user";

export class UserDto implements User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  countryCode: string | undefined;
  createdAt: Timestamp | undefined;
  blockedAt: Timestamp | undefined;
  deletedAt: Timestamp | undefined;
  verifiedAt: Timestamp | undefined;
}
