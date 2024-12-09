import { Metadata, ServerUnaryCall } from "@grpc/grpc-js";
import { Injectable } from "@nestjs/common";
import {
  DeleteUserRequest,
  DeleteUserResponse,
  GetUserRequest,
  ListUsersRequest,
  ListUsersResponse,
  UpdateUserRequest,
  UserResponse,
  UserServiceController,
  UserServiceControllerMethods,
} from "src/proto/user/v1/user";
import { CreateUserDto } from "./dto/create-user.dto";
import { UsersService } from "./users.service";

@Injectable()
@UserServiceControllerMethods()
export class UsersController implements UserServiceController {
  constructor(private readonly usersService: UsersService) {}

  async createUser(
    data: CreateUserDto,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<UserResponse> {
    call.sendMetadata(metadata);
    const user = await this.usersService.createUser(data);
    return user;
  }

  async getUser(
    data: GetUserRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<UserResponse> {
    call.sendMetadata(metadata);
    const user = await this.usersService.getUser(data);
    return user;
  }

  async getAllUsers(
    data: ListUsersRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<ListUsersResponse> {
    call.sendMetadata(metadata);
    const users = await this.usersService.getAllUsers(data);
    console.log(users);
    return users;
  }

  async updateUser(
    data: UpdateUserRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<UserResponse> {
    console.log(data, "data");
    call.sendMetadata(metadata);
    const user = await this.usersService.updateUser(data);
    return user;
  }

  async deleteUser(
    data: DeleteUserRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<DeleteUserResponse> {
    call.sendMetadata(metadata);
    await this.usersService.deleteUser(data);
    return { success: true };
  }
}
