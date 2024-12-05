import { Injectable } from "@nestjs/common";
import { OperationStatus } from "../../proto/custom_enums/v1/custom_enums";
import { PingPongRequest, PingPongResponse } from "../../proto/test/v1/test";

@Injectable()
export class TestService {
  pingPong(data: PingPongRequest): PingPongResponse {
    return {
      status: OperationStatus.OPERATION_STATUS_OK,
      pong: `Response to: ${data.ping}`,
    };
  }
}
