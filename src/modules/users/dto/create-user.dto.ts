import { IsEmail, IsString } from "class-validator";
import { CreateUserRequest } from "src/proto/user/v1/user";

export class CreateUserDto implements CreateUserRequest {
  @IsString()
  firstName: string;

  @IsString()
  lastName: string;

  @IsString()
  countryCode: string;

  @IsEmail()
  email: string;
}
