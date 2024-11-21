import { Controller } from '@nestjs/common';
import { GrpcMethod } from '@nestjs/microservices';
import { TestService } from './test.service';
import { Metadata, ServerUnaryCall } from '@grpc/grpc-js';

@Controller('test')
export class TestController {
  constructor(private readonly testService: TestService) {}

  @GrpcMethod('TestService', 'PingPong') // Указываем имя сервиса и метода из .proto
  pingPong(
    data: { ping: string },
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): { pong: string } {
    const responseMetadata = new Metadata();
    responseMetadata.set(
      'Access-Control-Expose-Headers',
      'grpc-status, grpc-message',
    );

    // Отправляем метаданные клиенту
    call.sendMetadata(responseMetadata);
    return this.testService.pingPong(data);
  }
}
