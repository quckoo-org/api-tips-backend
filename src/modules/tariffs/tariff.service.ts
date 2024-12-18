import { Injectable } from "@nestjs/common";

import { Tariff } from "@prisma/client";
import { PrismaService } from "../../infrastructure/database/prisma/PrismaService";
import { GetTariffsResponse } from "../../proto/tariff/tariff";
import { Timestamp } from "../../proto/google/protobuf/timestamp";

@Injectable()
export class TariffService {
  constructor(private prisma: PrismaService) {}

  // async user(
  //   userWhereUniqueInput: Prisma.UserWhereUniqueInput,
  // ): Promise<User | null> {
  //   return this.prisma.user.findUnique({
  //     where: userWhereUniqueInput,
  //   });
  // }

  async getTariffs(params: {
    skip?: number;
    take?: number;
  }): Promise<GetTariffsResponse> {
    const { skip, take } = params;
    const tariffs = await this.prisma.tariff.findMany({
      skip,
      take,
    });

    const res: GetTariffsResponse = { tariffs: [] };

    // TODO: придумать конвертацию из datetime в protobuf timestamp
    const response = tariffs.map((tariff) => {
      res.tariffs.push({
        id: tariff.id,
        name: tariff.name,
        freeRequests: tariff.freeRequests,
        paidRequests: tariff.paidRequests,
        totalCost: tariff.totalCost,
        hidden: tariff.hidden,
        dateStart: { seconds: 0, nanos: 0 }, // <--
        dateExpiration: { seconds: 0, nanos: 0 }, // <--
      });
    });

    return res;
  }

  // async createUser(data: Prisma.UserCreateInput): Promise<User> {
  //   return this.prisma.user.create({
  //     data,
  //   });
  // }

  // async updateUser(params: {
  //   where: Prisma.UserWhereUniqueInput;
  //   data: Prisma.UserUpdateInput;
  // }): Promise<User> {
  //   const { where, data } = params;
  //   return this.prisma.user.update({
  //     data,
  //     where,
  //   });
  // }

  // async deleteUser(where: Prisma.UserWhereUniqueInput): Promise<User> {
  //   return this.prisma.user.delete({
  //     where,
  //   });
  // }
}
