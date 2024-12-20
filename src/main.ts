import { NestFactory } from "@nestjs/core";
import { Transport } from "@nestjs/microservices";
import { AppModule } from "./app.module";
import { ReflectionService } from "@grpc/reflection";
import { join } from "path";
import { GrpcCorsInterceptor } from "./core/shared/interceptors/GrpcCorsInterceptor";
import { RpcValidationExceptionFilter } from "./core/shared/filters/RpcExceptionsFilter";

import * as cookieParser from "cookie-parser";
import { Logger, ValidationPipe } from "@nestjs/common";

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  const logger = new Logger();
  const port_http1 = process.env.PORT_HTTP1 || 3000;
  const port_http2 = process.env.PORT_HTTP2 || 3001;

  // Настройка gRPC
  const grpcOptions = {
    transport: Transport.GRPC,
    options: {
      package: ["grpc.health.v1", "test", "custom_enums.v1", "grpc.user.v1"], // Specify your proto package name
      protoPath: [
        join(__dirname, "proto/health/v1/health.proto"),
        join(__dirname, "proto/test/v1/test.proto"),
        join(__dirname, "proto/custom_enums/v1/custom_enums.proto"),
        join(__dirname, "proto/user/v1/user.proto"),
      ],
      loader: {
        includeDirs: [join(__dirname, "proto/")], // Указываем директорию с proto
        longs: Number,
        enums: String,
      },
      url: `0.0.0.0:${port_http2}`,
      onLoadPackageDefinition: (pkg, server) => {
        logger.log("Available gRPC services:");
        Object.keys(pkg).forEach((serviceName) => {
          logger.log(`- ${serviceName}`);
        });
        logger.log(`Started grpc service at ${port_http2}`);
        new ReflectionService(pkg).addToServer(server);
      },
    },
  };
  // Подключение cookie-parser
  app.use(cookieParser("my-secret"));

  app.useGlobalInterceptors(new GrpcCorsInterceptor());
  app.useGlobalFilters(new RpcValidationExceptionFilter());
  app.connectMicroservice(grpcOptions);
  app.useGlobalPipes(new ValidationPipe());

  app.enableCors({
    credentials: true,
    methods: "*",
    origin: ["http://localhost:3000", "https://beta.api-tips.quckoo.net"],
    allowedHeaders: [
      "keep-alive",
      "user-agent",
      "cache-control",
      "content-type",
      "content-transfer-encoding",
      "custom-header-1",
      "x-accept-content-transfer-encoding",
      "x-accept-response-streaming",
      "x-user-agent",
      "x-grpc-web",
      "grpc-timeout",
      "authorization",
      "grpc-method-query-time",
      "cookie",
      "set-cookie",
    ],
  });
  // Запуск HTTP и gRPC на одном порту
  await app.listen(`${port_http1}`, "0.0.0.0", () =>
    logger.log(`Started grpc service at ${port_http1}`),
  );
  await app.startAllMicroservices();
}

bootstrap();
