import { UpdateUserRequest } from "src/proto/user/v1/user";
import {
  IsBoolean,
  IsEmail,
  IsISO31661Alpha3,
  IsOptional,
  IsString,
  Length,
} from "class-validator";

export class UpdateUserDto implements UpdateUserRequest {
  id: number;

  @IsOptional()
  @IsISO31661Alpha3()
  countryCode?: string;

  @IsOptional()
  @IsEmail()
  email?: string;

  @IsOptional()
  @IsString()
  @Length(1, 100)
  firstName?: string;

  @IsOptional()
  @IsString()
  @Length(1, 100)
  lastName?: string;

  @IsOptional()
  @IsBoolean()
  isBlocked?: boolean;

  @IsOptional()
  @IsBoolean()
  isDeleted?: boolean;

  @IsOptional()
  @IsBoolean()
  isVerified?: boolean;
}
