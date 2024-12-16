import { IsEmail, IsString } from "class-validator";
import { CreateUserRequest } from "src/proto/user/v1/user";

export class CreateUserDto implements CreateUserRequest {
  @IsString()
  fistName: string;

  @IsString()
  lastName: string;

  @IsString()
  cca3: string;

  @IsEmail()
  email: string;
}
