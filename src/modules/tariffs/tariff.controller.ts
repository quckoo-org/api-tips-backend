import { Controller } from "@nestjs/common";
import { GrpcMethod } from "@nestjs/microservices";
// import { GetTariffsRequest, GetTariffsResponse } from "../proto/tariff/tariff";
import { Metadata, ServerUnaryCall } from "@grpc/grpc-js";
import { TariffService } from "./tariff.service";
import {
  GetTariffsRequest,
  GetTariffsResponse,
} from "../../proto/tariff/tariff";

@Controller()
export class TariffController {
  constructor(private tariffService: TariffService) {}

  @GrpcMethod("TariffsService")
  async getTariffs(
    data: GetTariffsRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<GetTariffsResponse> {
    // const response: GetTariffsResponse = {
    //   tariffs: [
    //     {
    //       id: 1,
    //       name: "SuperTariff",
    //       freeRequests: 1,
    //       paidRequests: 10,
    //       totalCost: 11,
    //       hidden: false,
    //       dateStart: null,
    //       dateExpiration: null,
    //     },
    //     {
    //       id: 1,
    //       name: "SuperTariff",
    //       freeRequests: 1,
    //       paidRequests: 10,
    //       totalCost: 11,
    //       hidden: false,
    //       dateStart: null,
    //       dateExpiration: null,
    //     },
    //   ],
    // };

    const res = this.tariffService.getTariffs({});

    return res;
  }

  // @Get("test")
  // async test() {
  //   const res = await this.tariffService.getTariffs({});
  //
  //   return res;
  // }
}
