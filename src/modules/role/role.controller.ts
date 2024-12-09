/* eslint-disable @typescript-eslint/no-unused-vars */
import {
  CreateRoleRequest,
  DeleteRoleRequest,
  DeleteRoleResponse,
  GetRoleRequest,
  ListRolesRequest,
  ListRolesResponse,
  RoleResponse,
  RoleServiceController,
  RoleServiceControllerMethods,
  UpdateRoleRequest,
} from "src/proto/role/v1/role";
import { RoleService } from "./role.service";
import { Metadata } from "@grpc/grpc-js";
import { Observable } from "rxjs";

@RoleServiceControllerMethods()
export class RoleController implements RoleServiceController {
  constructor(private readonly roleService: RoleService) {}
  createRole(
    request: CreateRoleRequest,
    metadata: Metadata,
    ...rest: any
  ): Promise<RoleResponse> | Observable<RoleResponse> | RoleResponse {
    throw new Error("Method not implemented.");
  }
  getRole(
    request: GetRoleRequest,
    metadata: Metadata,
    ...rest: any
  ): Promise<RoleResponse> | Observable<RoleResponse> | RoleResponse {
    throw new Error("Method not implemented.");
  }
  getAllRoles(
    request: ListRolesRequest,
    metadata: Metadata,
    ...rest: any
  ):
    | Promise<ListRolesResponse>
    | Observable<ListRolesResponse>
    | ListRolesResponse {
    throw new Error("Method not implemented.");
  }
  updateRole(
    request: UpdateRoleRequest,
    metadata: Metadata,
    ...rest: any
  ): Promise<RoleResponse> | Observable<RoleResponse> | RoleResponse {
    throw new Error("Method not implemented.");
  }
  deleteRole(
    request: DeleteRoleRequest,
    metadata: Metadata,
    ...rest: any
  ):
    | Promise<DeleteRoleResponse>
    | Observable<DeleteRoleResponse>
    | DeleteRoleResponse {
    throw new Error("Method not implemented.");
  }
}
