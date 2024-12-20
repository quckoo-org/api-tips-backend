import { IsEmail, IsNotEmpty, IsString, MinLength } from "class-validator";
import { User } from "@prisma/client";

export class RegisterDto
  implements Pick<User, "firstName" | "lastName" | "email">
{
  @IsEmail()
  @IsNotEmpty({ message: "Email can't be empty" })
  email: string;

  @IsNotEmpty()
  @MinLength(8, { message: "Password must be at least 8 characters long" })
  password: string;

  @IsString()
  firstName: string;

  @IsString()
  lastName: string;
}
