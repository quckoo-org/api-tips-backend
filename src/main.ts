import { NestFactory } from "@nestjs/core";
import { Transport } from "@nestjs/microservices";
import { AppModule } from "./app.module";
import { ReflectionService } from "@grpc/reflection";
import { join } from "path";
import { GrpcCorsInterceptor } from "./core/shared/interceptors/GrpcCorsInterceptor";
import { RpcValidationExceptionFilter } from "./core/shared/filters/RpcExceptionsFilter";

import * as cookieParser from "cookie-parser";

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Настройка gRPC
  const grpcOptions = {
    transport: Transport.GRPC,
    options: {
      package: ["grpc.health.v1", "test", "custom_enums.v1"], // Specify your proto package name
      protoPath: [
        join(__dirname, "proto/health/v1/health.proto"),
        join(__dirname, "proto/test/v1/test.proto"),
        join(__dirname, "proto/custom_enums/v1/custom_enums.proto"),
      ],
      loader: {
        includeDirs: [join(__dirname, "proto/")], // Указываем директорию с proto
      },
      url: "0.0.0.0:6000", // Set gRPC server URL,
      onLoadPackageDefinition: (pkg, server) => {
        console.log("Available gRPC services:");
        Object.keys(pkg).forEach((serviceName) => {
          console.log(`- ${serviceName}`);
        });
        console.log(
          "Started service at ",
          new Date().toUTCString(),
          " on port 3000",
        );
        new ReflectionService(pkg).addToServer(server);
      },
    },
  };
  // Подключение cookie-parser
  app.use(cookieParser("my-secret")); // Опционально: добавьте секрет для подписанных кук

  app.useGlobalInterceptors(new GrpcCorsInterceptor());
  app.useGlobalFilters(new RpcValidationExceptionFilter());
  app.connectMicroservice(grpcOptions);
  app.enableCors({
    credentials: true,
    methods: "*",
    origin: "*",
    allowedHeaders: "*",
  });
  // Запуск HTTP и gRPC на одном порту
  await app.listen(3000);
  await app.startAllMicroservices();
}

bootstrap();
