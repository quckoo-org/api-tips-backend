import { Timestamp } from "src/proto/google/protobuf/timestamp";
import { User } from "src/proto/user/v1/user";

export class UserDto implements User {
  email: string;
  fistName: string;
  lastName: string;
  cca3: string;
  id: number;
  isBlocked: Timestamp;
  isDeleted: Timestamp;
  isVerified: Timestamp;
  createdAt: Timestamp;
}
