import { Metadata, StatusBuilder } from "@grpc/grpc-js";
import { RpcException } from "@nestjs/microservices";
import { Status } from "@grpc/grpc-js/build/src/constants";

export class RpcExceptionBuilder extends Error {
  constructor(message: string, code: Status, metadata?: Metadata) {
    super(message);
    this.name = "RpcException";
    const status = new StatusBuilder()
      .withCode(code)
      .withDetails(message)
      .withMetadata(metadata)
      .build();

    throw new RpcException(status);
  }
}
