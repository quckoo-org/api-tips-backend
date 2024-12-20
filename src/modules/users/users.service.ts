import { Mapper } from "@automapper/core";
import { InjectMapper } from "@automapper/nestjs";
import { Injectable } from "@nestjs/common";
import * as bcrypt from "bcrypt";
import { ValidationUtil } from "src/core/shared/validation/validation.util";
import { PrismaService } from "src/infrastructure/database/prisma/prisma.service";
import { OperationStatus } from "src/proto/custom_enums/v1/custom_enums";
import type {
  CreateUserRequest,
  CreateUserResponse,
  GetAllUsersRequest,
  GetAllUsersResponse,
  GetCurrentUserRequest,
  GetCurrentUserResponse,
  GetUserRequest,
  GetUserResponse,
  UpdateUserRequest,
  UpdateUserResponse,
  UserServiceImplementation,
} from "src/proto/user/v1/user";
import { CreateUserDto } from "./dto/create-user.dto";
import { UpdateUserDto } from "./dto/update-user.dto";
import { UserDto } from "./dto/user.dto";
import { UserEntity } from "./dto/user.entity";
import { JwtService } from "@nestjs/jwt";
import * as process from "node:process";

@Injectable()
export class UsersService implements UserServiceImplementation {
  constructor(
    private readonly prisma: PrismaService,
    private readonly jwtService: JwtService,
    @InjectMapper() private readonly mapper: Mapper,
  ) {}

  async createUser(request: CreateUserRequest): Promise<CreateUserResponse> {
    const response: CreateUserResponse = {
      user: null,
      description: null,
      status: OperationStatus.OPERATION_STATUS_UNSPECIFIED,
    };

    const createUserDto = await ValidationUtil.validateAndThrow(
      CreateUserDto,
      request,
    );

    const candidate = await this.getUsersByEmail(createUserDto.email);

    if (candidate) {
      response.status = OperationStatus.OPERATION_STATUS_ERROR;
      response.description = "A user with this email already exists";

      return response;
    }

    const hashPassword = await bcrypt.hash(new Date().toString(), 5);

    const user = await this.prisma.user.create({
      data: {
        email: createUserDto.email,
        firstName: createUserDto.firstName,
        lastName: createUserDto.lastName,
        countryCode: createUserDto.countryCode,
        password: hashPassword,
      },
    });

    response.status = OperationStatus.OPERATION_STATUS_OK;
    response.user = this.mapper.map(user, UserEntity, UserDto);

    return response;
  }

  async getAllUsers(request: GetAllUsersRequest): Promise<GetAllUsersResponse> {
    const response: GetAllUsersResponse = {
      users: null,
      totalCount: null,
      currentPage: null,
      totalPages: null,
      description: null,
      status: OperationStatus.UNRECOGNIZED,
    };

    console.log(request);

    const page = request.page || 1;
    const pageSize = request.pageSize || 10;

    const skip = (page - 1) * pageSize;
    const take = pageSize;

    const filters = {} as any;

    if (request.isBlocked !== undefined)
      filters.blockedAt = request.isBlocked ? { not: null } : null;
    if (request.isDeleted !== undefined)
      filters.deletedAt = request.isDeleted ? { not: null } : null;
    if (request.isVerified !== undefined)
      filters.verifiedAt = request.isVerified ? { not: null } : null;

    if (request.searchByEmail && request.searchByEmail.trim() !== "") {
      filters.email = {
        contains: request.searchByEmail.trim(),
        mode: "insensitive",
      };
    }

    const orderBy = [];

    if (request.orderBy) {
      const order = {
        [request.orderBy]: request.order ? request.order : "asc",
      };

      orderBy.push(order);
    }

    const users = await this.prisma.user.findMany({
      skip,
      take,
      where: filters,
      orderBy,
    });

    const totalCount = await this.prisma.user.count({ where: filters });

    const totalPages = Math.ceil(totalCount / pageSize);

    response.users = users.map((user) =>
      this.mapper.map(user, UserEntity, UserDto),
    );
    response.totalCount = totalCount;
    response.totalPages = totalPages;
    response.currentPage = page;

    return response;
  }

  async getUser(request: GetUserRequest): Promise<GetUserResponse> {
    const response: GetUserResponse = {
      user: null,
      description: null,
      status: OperationStatus.OPERATION_STATUS_UNSPECIFIED,
    };

    const user = await this.prisma.user.findUnique({
      where: { id: request.userId },
    });

    if (!user) {
      response.description = "user not found";
      response.status = OperationStatus.OPERATION_STATUS_NO_DATA;
      return response;
    }

    response.status = OperationStatus.OPERATION_STATUS_OK;
    response.user = this.mapper.map(user, UserEntity, UserDto);

    return response;
  }

  async getCurrentUser(bearerToken: string): Promise<GetCurrentUserResponse> {
    const response: GetUserResponse = {
      user: null,
      description: null,
      status: OperationStatus.OPERATION_STATUS_UNSPECIFIED,
    };

    let token = "";

    if (bearerToken.startsWith("Bearer ")) {
      token = bearerToken.substring(7, bearerToken.length);
    } else {
      response.description = "missing token";
      response.status = OperationStatus.OPERATION_STATUS_DECLINED;

      return response;
    }

    try {
      const payload = this.jwtService.verify(token, {
        secret: process.env.JWT_SECRET || "secretKey",
      });

      const user = await this.prisma.user.findUnique({
        where: { id: payload.sub },
      });

      response.status = OperationStatus.OPERATION_STATUS_OK;
      response.user = this.mapper.map(user, UserEntity, UserDto);

      return response;
    } catch (err) {
      console.log(err);
      response.description = "user not found";
      response.status = OperationStatus.OPERATION_STATUS_NO_DATA;
      return response;
    }
  }

  async updateUser(request: UpdateUserRequest): Promise<UpdateUserResponse> {
    const response: UpdateUserResponse = {
      user: null,
      description: undefined,
      status: OperationStatus.UNRECOGNIZED,
    };

    const updateUserDto = await ValidationUtil.validateAndThrow(
      UpdateUserDto,
      request,
    );

    const userEntity = this.mapper.map(
      updateUserDto,
      UpdateUserDto,
      UserEntity,
    );

    const existingUser = await this.prisma.user.findUnique({
      where: { id: request.id },
    });

    if (!existingUser) {
      response.status = OperationStatus.OPERATION_STATUS_ERROR;
      response.description = `User with id ${request.id} does not exist`;

      return response;
    }

    const updatedUser = await this.prisma.user.update({
      where: { id: request.id },
      data: userEntity,
    });

    response.user = this.mapper.map(updatedUser, UserEntity, UserDto);
    response.status = OperationStatus.OPERATION_STATUS_OK;

    return response;
  }

  private async getUsersByEmail(email: string) {
    const user = await this.prisma.user.findUnique({
      where: { email: email },
    });

    return user;
  }
}
