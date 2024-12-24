import { IsEmail, IsISO31661Alpha3, IsString, Length } from "class-validator";
import { CreateUserRequest } from "src/proto/user/v1/user";

export class CreateUserDto implements CreateUserRequest {
  @IsString()
  @Length(1, 100)
  firstName: string;

  @IsString()
  @Length(1, 100)
  lastName: string;

  @IsISO31661Alpha3()
  countryCode: string;

  @IsEmail()
  email: string;
}
