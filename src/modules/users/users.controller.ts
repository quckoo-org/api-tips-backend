import { Metadata, ServerUnaryCall } from "@grpc/grpc-js";
import { Injectable } from "@nestjs/common";
import {
  GetUserRequest,
  UpdateUserRequest,
  CreateUserResponse,
  UpdateUserResponse,
  UserServiceController,
  UserServiceControllerMethods,
  GetUserResponse,
  GetAllUsersRequest,
  GetAllUsersResponse,
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
  ): Promise<CreateUserResponse> {
    call.sendMetadata(metadata);
    const user = await this.usersService.createUser(data);
    return user;
  }

  async getUser(
    data: GetUserRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<GetUserResponse> {
    call.sendMetadata(metadata);
    const user = await this.usersService.getUser(data);
    return user;
  }

  async getAllUsers(
    data: GetAllUsersRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<GetAllUsersResponse> {
    call.sendMetadata(metadata);
    const users = await this.usersService.getAllUsers(data);
    return users;
  }

  async updateUser(
    data: UpdateUserRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<UpdateUserResponse> {
    console.log(data, "data");
    call.sendMetadata(metadata);
    const user = await this.usersService.updateUser(data);
    return user;
  }
}
