import { Metadata } from "@grpc/grpc-js";
import { Controller, Injectable, UseGuards } from "@nestjs/common";
import {
  CreateUserResponse,
  GetAllUsersRequest,
  GetAllUsersResponse,
  GetCurrentUserResponse,
  GetUserRequest,
  GetUserResponse,
  UpdateUserRequest,
  UpdateUserResponse,
  UserServiceController,
  UserServiceControllerMethods,
} from "src/proto/user/v1/user";
import { CreateUserDto } from "./dto/create-user.dto";
import { UsersService } from "./users.service";
import { GrpcAuthGuard } from "../auth/grpc-auth.guard";

@Injectable()
@Controller()
@UserServiceControllerMethods()
export class UsersController implements UserServiceController {
  constructor(private readonly usersService: UsersService) {}

  @UseGuards(GrpcAuthGuard)
  async createUser(data: CreateUserDto): Promise<CreateUserResponse> {
    const user = await this.usersService.createUser(data);
    return user;
  }

  @UseGuards(GrpcAuthGuard)
  async getUser(data: GetUserRequest): Promise<GetUserResponse> {
    const user = await this.usersService.getUser(data);
    return user;
  }

  @UseGuards(GrpcAuthGuard)
  async getCurrentUser(metadata: Metadata): Promise<GetCurrentUserResponse> {
    const bearer: string = metadata.get("authorization").toString();
    const user = await this.usersService.getCurrentUser(bearer);
    return user;
  }

  @UseGuards(GrpcAuthGuard)
  async getAllUsers(data: GetAllUsersRequest): Promise<GetAllUsersResponse> {
    const users = await this.usersService.getAllUsers(data);
    return users;
  }

  @UseGuards(GrpcAuthGuard)
  async updateUser(data: UpdateUserRequest): Promise<UpdateUserResponse> {
    const user = await this.usersService.updateUser(data);
    return user;
  }
}
