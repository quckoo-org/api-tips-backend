import { Injectable } from '@nestjs/common';
import { PingPongRequest, PingPongResponse } from '../../proto/test/v1/test';

@Injectable()
export class TestService {
  pingPong(data: PingPongRequest): PingPongResponse {
    return { pong: `Response to: ${data.ping}` };
  }
}
