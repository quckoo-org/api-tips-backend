import { StatusBuilder } from "@grpc/grpc-js";
import { RpcException } from "@nestjs/microservices";

export class RpcExceptionBuilder extends Error {
  constructor(message: string, code: number) {
    super(message);
    this.name = "RpcException";
    const status = new StatusBuilder()
      .withCode(code)
      .withDetails(message)
      .build();

    throw new RpcException(status);
  }
}
