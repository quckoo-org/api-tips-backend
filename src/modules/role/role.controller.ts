import { RoleService } from "./role.service";
import { Controller } from "@nestjs/common";

@Controller()
export class RoleController {
  constructor(private readonly roleService: RoleService) {}

  // async getAllRoles(
  //   request: GetAllRolesRequest,
  //   metadata: Metadata,
  //   call: ServerUnaryCall<any, any>,
  // ): Promise<GetAllRolesResponse> {
  //   call.sendMetadata(metadata);
  //   return await this.roleService.getAllRoles(request);
  // }
}
