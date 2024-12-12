import { Metadata, ServerUnaryCall } from "@grpc/grpc-js";
import {
  GetAllRolesRequest,
  GetAllRolesResponse,
  RoleServiceController,
  RoleServiceControllerMethods,
} from "src/proto/role/v1/role";
import { RoleService } from "./role.service";

@RoleServiceControllerMethods()
export class RoleController implements RoleServiceController {
  constructor(private readonly roleService: RoleService) {}

  async getAllRoles(
    request: GetAllRolesRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<GetAllRolesResponse> {
    call.sendMetadata(metadata);
    return await this.roleService.getAllRoles(request);
  }
}
