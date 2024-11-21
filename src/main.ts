import { NestFactory } from '@nestjs/core';
import { MicroserviceOptions, Transport } from '@nestjs/microservices';
import { AppModule } from './app.module';
import { ReflectionService } from '@grpc/reflection';
import { join } from 'path';

import { GrpcCorsInterceptor } from './common/interceptors/GrpcCorsInterceptor';

// // Get the current module's directory path
// const __filename = fileURLToPath(import.meta.url);
// const __dirname = path.dirname(__filename);
async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  app.connectMicroservice<MicroserviceOptions>({
    transport: Transport.GRPC,
    options: {
      package: 'test', // Specify your proto package name
      protoPath: join(__dirname, 'proto/test/v1/test.proto'), // Path to your .proto file
      url: '0.0.0.0:3000', // Set gRPC server URL,
      onLoadPackageDefinition: (pkg, server) => {
        console.log(
          'Started service at ',
          new Date().toUTCString(),
          ' on port http://0.0.0.0:3000',
        );
        new ReflectionService(pkg).addToServer(server);
      },
    },
  });

  // app.setGlobalPrefix('api')
  // app.enableCors({
  //   "origin": "*",
  //   "methods": "GET,HEAD,PUT,PATCH,POST,DELETE",
  //   "preflightContinue": false,
  // })
  app.useGlobalInterceptors(new GrpcCorsInterceptor());
  app.startAllMicroservices();
}

bootstrap();
