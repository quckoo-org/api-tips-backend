import { Injectable } from "@nestjs/common";
import { PrismaService } from "src/infrastructure/database/prisma/prisma.service";

@Injectable()
export class RoleService {
  constructor(private readonly prisma: PrismaService) {}

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  // async getAllRoles(request: GetAllRolesRequest): Promise<GetAllRolesResponse> {
  //   const response: GetAllRolesResponse = {
  //     roles: null,
  //     description: null,
  //   };

  //   return response;
  //   // const roles = await this.prisma.role.findMany();

  //   // return { roles };
  // }

  // public async getRolesByIds(roleIds: number[]): Promise<Role[]> {
  //   return this.prisma.role.findMany({
  //     where: {
  //       id: { in: roleIds },
  //     },
  //   });
  // }
}
