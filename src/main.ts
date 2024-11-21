import { NestFactory } from '@nestjs/core';
import { MicroserviceOptions, Transport } from '@nestjs/microservices';
import { AppModule } from './app.module';
import { ReflectionService } from '@grpc/reflection';
import { join, resolve } from 'path';

import { GrpcCorsInterceptor } from './common/interceptors/GrpcCorsInterceptor';

// // Get the current module's directory path
// const __filename = fileURLToPath(import.meta.url);
// const __dirname = path.dirname(__filename);
async function bootstrap() {
  const app = await NestFactory.createMicroservice(AppModule, {
    transport: Transport.GRPC,
    options: {
      package: ['grpc.health.v1', 'test', 'custom_enums.v1'], // Specify your proto package name
      protoPath: [
        join(__dirname, 'proto/health/v1/health.proto'),
        join(__dirname, 'proto/test/v1/test.proto'),
        join(__dirname, 'proto/custom_enums/v1/custom_enums.proto'),
      ],
      loader: {
        includeDirs: [join(__dirname, 'proto/')], // Указываем директорию с proto
      },
      url: '0.0.0.0:3000', // Set gRPC server URL,
      onLoadPackageDefinition: (pkg, server) => {
        console.log(
          'Started service at ',
          new Date().toUTCString(),
          ' on port 3000',
        );
        new ReflectionService(pkg).addToServer(server);
      },
    },
  });

  app.useGlobalInterceptors(new GrpcCorsInterceptor());
  app.listen();
}

bootstrap();
