import { Injectable } from '@nestjs/common';
import { PingPongRequest, PingPongResponse } from '../../proto/test/v1/test';
import { OperationStatus } from '../../proto/custom_enums/v1/custom_enums';
import { StatusBuilder } from '@grpc/grpc-js';
import { Status } from '@grpc/grpc-js/build/src/constants';

import { RpcException } from '@nestjs/microservices';

@Injectable()
export class TestService {
  pingPong(data: PingPongRequest): PingPongResponse {
    return {
      status: OperationStatus.OPERATION_STATUS_OK,
      pong: `Response to: ${data.ping}`,
    };
  }
}
