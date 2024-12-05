import {
  IsString,
  IsEmail,
  IsBoolean,
  IsOptional,
  IsArray,
} from "class-validator";
import { CreateUserRequest } from "src/proto/user/v1/user";

export class CreateUserDto implements CreateUserRequest {
  @IsEmail()
  email: string;

  @IsString()
  name: string;

  @IsString()
  lastname: string;

  @IsOptional()
  @IsBoolean()
  isDeleted?: boolean;

  @IsOptional()
  @IsBoolean()
  isBlocked?: boolean;

  @IsOptional()
  @IsBoolean()
  isHidden?: boolean;

  @IsOptional()
  @IsString()
  lastIp?: string;

  @IsOptional()
  @IsBoolean()
  isVerified?: boolean;

  @IsString()
  @IsOptional()
  token?: string;

  @IsString()
  @IsOptional()
  country?: string;

  @IsArray()
  roles: number[];
}
