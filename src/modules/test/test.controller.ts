import { Controller, UseInterceptors } from "@nestjs/common";
import { GrpcMethod, RpcException } from "@nestjs/microservices";
import { TestService } from "./test.service";
import {
  PingPongRequest,
  PingPongResponse,
  TestServiceController,
  TestServiceControllerMethods,
} from "../../proto/test/v1/test";
import { Metadata, ServerUnaryCall, StatusBuilder } from "@grpc/grpc-js";
import { Status } from "@grpc/grpc-js/build/src/constants";
import { PrismaService } from "../../infrastructure/database/prisma/prisma.service";

@Controller()
@TestServiceControllerMethods()
export class TestController implements TestServiceController {
  constructor(
    private readonly testService: TestService,
    private readonly prismaService: PrismaService,
  ) {}

  pingPong(
    data: PingPongRequest,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): PingPongResponse {
    // Создаем метаданные для отправки в ответе
    call.sendMetadata(metadata);
    console.log("asdasd");
    if (!data.ping) {
      // Создаем новый объект StatusBuilder
      const status = new StatusBuilder()
        .withCode(Status.ABORTED) // Указываем код ошибки
        .withDetails("Invalid argument: missing someField")
        .build(); // Строим окончательный статус

      // Выбрасываем RpcException с созданным статусом

      throw new RpcException(status);
    }

    return this.testService.pingPong(data);
  }
}
