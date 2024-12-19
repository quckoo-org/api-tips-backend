import { ReflectionService } from "@grpc/reflection";
import { Logger } from "@nestjs/common";
import { NestFactory } from "@nestjs/core";
import { Transport } from "@nestjs/microservices";
import { join } from "path";
import { AppModule } from "./app.module";
import { RpcValidationExceptionFilter } from "./core/shared/filters/RpcExceptionsFilter";
import { GrpcCorsInterceptor } from "./core/shared/interceptors/GrpcCorsInterceptor";

// // Get the current module's directory path
// const __filename = fileURLToPath(import.meta.url);
// const __dirname = path.dirname(__filename);
async function bootstrap() {
  const logger = new Logger("Bootstrap");
  const PORT = process.env.PORT || 3000;
  const HOST = process.env.HOST || "localhost";

  const app = await NestFactory.createMicroservice(AppModule, {
    transport: Transport.GRPC,
    options: {
      package: ["grpc.user.v1", "test", "custom_enums.v1"], // Specify your proto package name
      protoPath: [
        join(__dirname, "proto/user/v1/user.proto"),
        join(__dirname, "proto/test/v1/test.proto"),
        join(__dirname, "proto/custom_enums/v1/custom_enums.proto"),
      ],
      loader: {
        includeDirs: [join(__dirname, "proto/")], // Указываем директорию с proto
        longs: Number,
        enums: String,
      },
      url: `${HOST}:${PORT}`, // Set gRPC server URL,
      onLoadPackageDefinition: (pkg, server) => {
        console.log(
          "Started service at ",
          new Date().toUTCString(),
          ` on port ${PORT}`,
        );
        new ReflectionService(pkg).addToServer(server);
      },
    },
  });

  app.useGlobalInterceptors(new GrpcCorsInterceptor());
  app.useGlobalFilters(new RpcValidationExceptionFilter());
  await app.listen();

  logger.log(`Application is running`);
}

bootstrap();
