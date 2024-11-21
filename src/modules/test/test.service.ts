import { Injectable } from '@nestjs/common';
import { PingPongRequest, PingPongResponse } from '../../proto/test/v1/test';
import { OperationStatus } from '../../proto/custom_enums/v1/custom_enums';

@Injectable()
export class TestService {
  pingPong(data: PingPongRequest): PingPongResponse {
    // Создаем метаданные для отправки в ответе

    return {
      status: OperationStatus.OPERATION_STATUS_OK,
      pong: `Response to: ${data.ping}`,
    };
  }
}
