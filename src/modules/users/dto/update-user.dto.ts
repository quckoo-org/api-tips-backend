import { UpdateUserRequest } from "src/proto/user/v1/user";

export class UpdateUserDto implements UpdateUserRequest {
  id: number;
  countryCode?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  isBlocked?: boolean;
  isDeleted?: boolean;
  isVerified?: boolean;
}
