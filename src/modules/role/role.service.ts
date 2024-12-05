/* eslint-disable @typescript-eslint/no-unused-vars */
import { Injectable } from "@nestjs/common";
import { Role } from "@prisma/client";
import { PrismaService } from "src/infrastructure/database/prisma/PrismaService";
import {
  CreateRoleRequest,
  DeleteRoleRequest,
  DeleteRoleResponse,
  GetRoleRequest,
  ListRolesRequest,
  ListRolesResponse,
  RoleResponse,
  RoleServiceImplementation,
  UpdateRoleRequest,
} from "src/proto/role/v1/role";

@Injectable()
export class RoleService implements RoleServiceImplementation {
  constructor(private readonly prisma: PrismaService) {}
  createRole(request: CreateRoleRequest): Promise<RoleResponse> {
    throw new Error("Method not implemented.");
  }

  async getRole(request: GetRoleRequest): Promise<RoleResponse> {
    const role = await this.prisma.role.findUnique({
      where: { id: request.id },
    });

    return { role: role };
  }

  getAllRoles(request: ListRolesRequest): Promise<ListRolesResponse> {
    throw new Error("Method not implemented.");
  }
  updateRole(request: UpdateRoleRequest): Promise<RoleResponse> {
    throw new Error("Method not implemented.");
  }
  deleteRole(request: DeleteRoleRequest): Promise<DeleteRoleResponse> {
    throw new Error("Method not implemented.");
  }

  public async getRoleByValue(value: string): Promise<RoleResponse> {
    const role = await this.prisma.role.findUnique({
      where: { value: value },
    });

    return { role: role };
  }

  public async getRolesByIds(roleIds: number[]): Promise<Role[]> {
    return this.prisma.role.findMany({
      where: {
        id: { in: roleIds },
      },
    });
  }
}
