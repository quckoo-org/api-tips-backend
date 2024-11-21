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
      'grpc-status,grpc-message',
    );
    responseMetadata.set('Access-Control-Allow-Origin', '*');
    responseMetadata.set('Access-Control-Allow-Credentials', 'true');
    responseMetadata.set(
      'Access-Control-Allow-Headers',
      'keep-alive,user-agent,cache-control,content-type,content-transfer-encoding,custom-header-1,x-accept-content-transfer-encoding,x-accept-response-streaming,x-user-agent,x-grpc-web,grpc-timeout, authorization',
    );
    responseMetadata.set('Access-Control-Allow-Methods', '*');

    // Отправляем метаданные клиенту
    call.sendMetadata(responseMetadata);
    return this.testService.pingPong(data);
  }
}
