import { Status } from "@grpc/grpc-js/build/src/constants";
import { Injectable } from "@nestjs/common";
import { Role, User } from "@prisma/client";
import * as bcrypt from "bcryptjs";
import { RpcExceptionBuilder } from "src/core/shared/exception/rpc-exception.builder";
import { ValidationUtil } from "src/core/shared/validation/validation.util";
import { PrismaService } from "src/infrastructure/database/prisma/PrismaService";
import type {
  CreateUserRequest,
  DeleteUserRequest,
  DeleteUserResponse,
  GetUserRequest,
  ListUsersRequest,
  ListUsersResponse,
  User as ProtoUser,
  UpdateUserRequest,
  UserResponse,
  UserServiceImplementation,
} from "src/proto/user/v1/user";
import { RoleService } from "../role/role.service";
import { CreateUserDto } from "./dto/create-user.dto";
import { UpdateUserDto } from "./dto/update-user.dto";

@Injectable()
export class UsersService implements UserServiceImplementation {
  constructor(
    private readonly prisma: PrismaService,
    private readonly roleService: RoleService,
  ) {}

  async createUser(request: CreateUserRequest): Promise<UserResponse> {
    const createUserDto = await ValidationUtil.validateAndThrow(
      CreateUserDto,
      request,
    );

    const candidate = await this.getUsersByEmail(createUserDto.email);

    if (candidate) {
      throw new RpcExceptionBuilder(
        "A user with this email already exists",
        Status.UNKNOWN,
      );
    }

    const hashPassword = await bcrypt.hash(new Date().toString(), 5);
    const roles = await this.roleService.getRolesByIds(createUserDto.roles);

    const user = await this.prisma.user.create({
      data: {
        email: createUserDto.email,
        name: createUserDto.name,
        lastname: createUserDto.lastname,
        token: createUserDto.token,
        country: createUserDto.country,
        isDeleted: false,
        isBlocked: false,
        isHidden: false,
        lastIp: createUserDto.lastIp,
        password: hashPassword,
        isVerified: createUserDto.isVerified || false,
        roles: {
          create: roles.map((role) => ({
            roleId: role.id,
          })),
        },
      },
      include: {
        roles: {
          include: {
            role: true,
          },
        },
      },
    });

    return { user: this.mapToProtoUser(user) };
  }

  async getAllUsers(request: ListUsersRequest): Promise<ListUsersResponse> {
    const page = request.page || 1;
    const pageSize = request.pageSize || 10;

    const skip = (page - 1) * pageSize;
    const take = pageSize;

    // Фильтры
    const filters: Omit<ListUsersRequest, "page" | "pageSize"> & {
      email?: {
        contains: string;
        mode: "insensitive";
      };
    } = {};

    if (request.isBlocked !== undefined) filters.isBlocked = request.isBlocked;
    if (request.isHidden !== undefined) filters.isHidden = request.isHidden;
    if (request.isDeleted !== undefined) filters.isDeleted = request.isDeleted;
    if (request.isVerified !== undefined)
      filters.isVerified = request.isVerified;

    if (request.search && request.search.trim() !== "") {
      filters.email = {
        contains: request.search.trim(),
        mode: "insensitive",
      };
    }

    const users = await this.prisma.user.findMany({
      skip,
      take,
      where: filters,
      include: {
        roles: {
          include: {
            role: true,
          },
        },
      },
    });

    const totalCount = await this.prisma.user.count({ where: filters });

    const totalPages = Math.ceil(totalCount / pageSize);

    return {
      rows: users.map(this.mapToProtoUser),
      totalCount,
      totalPages,
      currentPage: page,
    };
  }

  async getUser(request: GetUserRequest): Promise<UserResponse> {
    const user = await this.prisma.user.findUnique({
      where: { id: request.id },
      include: {
        roles: {
          include: {
            role: true,
          },
        },
      },
    });

    if (!user) {
      throw new RpcExceptionBuilder(
        `User with ID ${request.id} not found`,
        Status.NOT_FOUND,
      );
    }

    return { user: this.mapToProtoUser(user) };
  }

  async updateUser(request: UpdateUserRequest): Promise<UserResponse> {
    const updateUserDto = await ValidationUtil.validateAndThrow(
      UpdateUserDto,
      request,
    );

    const existingUser = await this.prisma.user.findUnique({
      where: { id: request.id },
      include: { roles: true },
    });

    if (!existingUser) {
      throw new RpcExceptionBuilder(
        `User with id ${request.id} does not exist`,
        Status.NOT_FOUND,
      );
    }

    const updatedData: any = {
      email: updateUserDto.email,
      name: updateUserDto.name,
      lastname: updateUserDto.lastname,
      token: updateUserDto.token,
      country: updateUserDto.country,
      isDeleted: updateUserDto.isDeleted,
      isBlocked: updateUserDto.isBlocked,
      isHidden: updateUserDto.isHidden,
      lastIp: updateUserDto.lastIp,
      isVerified: updateUserDto.isVerified,
    };

    if (updateUserDto.roles && updateUserDto.roles.length > 0) {
      const roles = await this.roleService.getRolesByIds(updateUserDto.roles);

      await this.prisma.userRole.deleteMany({
        where: { userId: request.id },
      });

      updatedData.roles = {
        create: roles.map((role) => ({
          roleId: role.id,
        })),
      };
    }

    const updatedUser = await this.prisma.user.update({
      where: { id: request.id },
      data: updatedData,
      include: {
        roles: {
          include: {
            role: true,
          },
        },
      },
    });

    return { user: this.mapToProtoUser(updatedUser) };
  }

  async deleteUser(request: DeleteUserRequest): Promise<DeleteUserResponse> {
    await this.prisma.user.delete({
      where: { id: request.id },
    });

    return { success: true };
  }

  private mapToProtoUser(
    user: { roles?: { userId: number; roleId: number; role: Role }[] } & User,
  ): ProtoUser {
    return {
      id: user.id,
      email: user.email,
      name: user.name || "",
      lastname: user.lastname || "",
      token: user.token || "",
      country: user.country || "",
      isDeleted: user.isDeleted,
      isBlocked: user.isBlocked,
      isHidden: user.isHidden,
      lastIp: user.lastIp || "",
      isVerified: user.isVerified,
      roles: user?.roles.map((role) => role.role) ?? [],
      createdAt: {
        nanos: (user.createdAt.valueOf() % 1000) * 1000000,
        seconds: Math.floor(user.createdAt.valueOf() / 1000),
      },
    };
  }

  private async getUsersByEmail(email: string) {
    const user = await this.prisma.user.findUnique({
      where: { email: email },
      include: {
        roles: {
          select: {
            role: true,
          },
        },
      },
    });

    return user;
  }
}
