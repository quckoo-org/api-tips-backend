import { Injectable } from "@nestjs/common";
import { PrismaService } from "src/infrastructure/database/prisma/PrismaService";
import { OperationStatus } from "src/proto/custom_enums/v1/custom_enums";
import type {
  CreateUserRequest,
  CreateUserResponse,
  GetAllUsersRequest,
  GetAllUsersResponse,
  GetUserRequest,
  GetUserResponse,
  UpdateUserRequest,
  UpdateUserResponse,
  UserServiceImplementation,
} from "src/proto/user/v1/user";
import { RoleService } from "../role/role.service";

@Injectable()
export class UsersService implements UserServiceImplementation {
  constructor(
    private readonly prisma: PrismaService,
    private readonly roleService: RoleService,
  ) {}

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async createUser(request: CreateUserRequest): Promise<CreateUserResponse> {
    const response: CreateUserResponse = {
      user: null,
      description: null,
      status: OperationStatus.UNRECOGNIZED,
    };

    return response;
    // const createUserDto = await ValidationUtil.validateAndThrow(
    //   CreateUserDto,
    //   request,
    // );
    // const candidate = await this.getUsersByEmail(createUserDto.email);
    // if (candidate) {
    //   throw new RpcExceptionBuilder(
    //     "A user with this email already exists",
    //     Status.UNKNOWN,
    //   );
    // }
    // const hashPassword = await bcrypt.hash(new Date().toString(), 5);
    // const roles = await this.roleService.getRolesByIds(createUserDto.roles);
    // const user = await this.prisma.user.create({
    //   data: {
    //     email: createUserDto.email,
    //     name: createUserDto.name,
    //     lastname: createUserDto.lastname,
    //     token: createUserDto.token,
    //     countryId: createUserDto.countryId,
    //     isDeleted: false,
    //     isBlocked: false,
    //     isHidden: false,
    //     lastIp: createUserDto.lastIp,
    //     password: hashPassword,
    //     isVerified: createUserDto.isVerified || false,
    //     roles: {
    //       create: roles.map((role) => ({
    //         roleId: role.id,
    //       })),
    //     },
    //   },
    //   include: {
    //     country: true,
    //     roles: {
    //       include: {
    //         role: true,
    //       },
    //     },
    //   },
    // });
    // return {
    //   user: this.mapToProtoUser(user),
    // };
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async getAllUsers(request: GetAllUsersRequest): Promise<GetAllUsersResponse> {
    const response: GetAllUsersResponse = {
      rows: null,
      totalCount: null,
      currentPage: null,
      totalPages: null,
      description: null,
      status: OperationStatus.UNRECOGNIZED,
    };

    return response;

    // const page = request.page || 1;
    // const pageSize = request.pageSize || 10;

    // const skip = (page - 1) * pageSize;
    // const take = pageSize;

    // const filters: Omit<ListUsersRequest, "page" | "pageSize"> & {
    //   email?: {
    //     contains: string;
    //     mode: "insensitive";
    //   };
    // } = {};

    // if (request.isBlocked !== undefined) filters.isBlocked = request.isBlocked;
    // if (request.isHidden !== undefined) filters.isHidden = request.isHidden;
    // if (request.isDeleted !== undefined) filters.isDeleted = request.isDeleted;
    // if (request.isVerified !== undefined)
    //   filters.isVerified = request.isVerified;

    // if (request.search && request.search.trim() !== "") {
    //   filters.email = {
    //     contains: request.search.trim(),
    //     mode: "insensitive",
    //   };
    // }

    // const users = await this.prisma.user.findMany({
    //   skip,
    //   take,
    //   where: filters,
    //   include: {
    //     country: true,
    //     roles: {
    //       include: {
    //         role: true,
    //       },
    //     },
    //   },
    // });

    // const totalCount = await this.prisma.user.count({ where: filters });

    // const totalPages = Math.ceil(totalCount / pageSize);

    // return {
    //   rows: users.map(this.mapToProtoUser),
    //   totalCount,
    //   totalPages,
    //   currentPage: page,
    // };
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async getUser(request: GetUserRequest): Promise<GetUserResponse> {
    const response: GetUserResponse = {
      user: null,
      description: null,
      status: OperationStatus.UNRECOGNIZED,
    };

    return response;

    // const user = await this.prisma.user.findUnique({
    //   where: { id: request.userId },
    //   include: {
    //     country: true,
    //     roles: {
    //       include: {
    //         role: true,
    //       },
    //     },
    //   },
    // });
    // if (!user) {
    //   throw new RpcExceptionBuilder(
    //     `User with ID ${request.userId} not found`,
    //     Status.NOT_FOUND,
    //   );
    // }
    // return {
    //   user: this.mapToProtoUser(user),
    // };
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async updateUser(request: UpdateUserRequest): Promise<UpdateUserResponse> {
    const response: UpdateUserResponse = {
      user: null,
      description: null,
    };

    return response;

    // const updateUserDto = await ValidationUtil.validateAndThrow(
    //   UpdateUserDto,
    //   request,
    // );

    // const existingUser = await this.prisma.user.findUnique({
    //   where: { id: request.id },
    //   include: { roles: true },
    // });

    // if (!existingUser) {
    //   throw new RpcExceptionBuilder(
    //     `User with id ${request.id} does not exist`,
    //     Status.NOT_FOUND,
    //   );
    // }

    // const updatedData: any = {
    //   email: updateUserDto.email,
    //   name: updateUserDto.name,
    //   lastname: updateUserDto.lastname,
    //   token: updateUserDto.token,
    //   countryId: updateUserDto.countryId,
    //   isDeleted: updateUserDto.isDeleted,
    //   isBlocked: updateUserDto.isBlocked,
    //   isHidden: updateUserDto.isHidden,
    //   lastIp: updateUserDto.lastIp,
    //   isVerified: updateUserDto.isVerified,
    // };

    // if (updateUserDto.roles && updateUserDto.roles.length > 0) {
    //   const roles = await this.roleService.getRolesByIds(updateUserDto.roles);

    //   await this.prisma.userRole.deleteMany({
    //     where: { userId: request.id },
    //   });

    //   updatedData.roles = {
    //     create: roles.map((role) => ({
    //       roleId: role.id,
    //     })),
    //   };
    // }

    // const updatedUser = await this.prisma.user.update({
    //   where: { id: request.id },
    //   data: updatedData,
    //   include: {
    //     country: true,
    //     roles: {
    //       include: {
    //         role: true,
    //       },
    //     },
    //   },
    // });

    // return { user: this.mapToProtoUser(updatedUser) };
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private mapToProtoUser(user: any) {
    // } & User, //   }; //     numericCode: string; //     alpha3Code: string; //     alpha2Code: string; //     name: string; //     id: number; //   country?: { //   roles?: { userId: number; roleId: number; role: Role }[]; // user: {
    // return {
    //   id: user.id,
    //   email: user.email,
    //   name: user.name || "",
    //   lastname: user.lastname || "",
    //   token: user.token || "",
    //   country: user?.country || null,
    //   isDeleted: user.isDeleted,
    //   isBlocked: user.isBlocked,
    //   isHidden: user.isHidden,
    //   isVerified: user.isVerified,
    //   lastIp: user.lastIp || "",
    //   roles: user?.roles.map((role) => role.role) ?? [],
    //   createdAt: {
    //     nanos: (user.createdAt.valueOf() % 1000) * 1000000,
    //     seconds: Math.floor(user.createdAt.valueOf() / 1000),
    //   },
    // };

    return {} as any;
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
