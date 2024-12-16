import { UpdateUserRequest } from "src/proto/user/v1/user";

export class UpdateUserDto implements UpdateUserRequest {
  id: number;
  cca3?: string;
  email?: string;
  firstName?: string;
  isBlocked?: boolean;
  isDeleted?: boolean;
  isVerified?: boolean;
  lastName?: string;
}
