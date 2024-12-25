import { IsEmail, IsNotEmpty, IsString, MinLength } from "class-validator";
import { User } from "@prisma/client";

export class RegisterDto
  implements Pick<User, "firstName" | "lastName" | "email" | "countryCode">
{
  @IsEmail()
  @IsNotEmpty({ message: "Email can't be empty" })
  email: string;

  @IsNotEmpty()
  @MinLength(8, { message: "Password must be at least 8 characters long" })
  password: string;

  @IsNotEmpty()
  @IsString()
  firstName: string;

  @IsNotEmpty()
  @IsString()
  lastName: string;

  @IsNotEmpty()
  @IsString()
  countryCode: string;
}
