import { ExecutionContext, Injectable } from "@nestjs/common";
import { JwtService } from "@nestjs/jwt";
import { RpcExceptionBuilder } from "../../core/shared/exception/rpc-exception.builder";
import { Status } from "@grpc/grpc-js/build/src/constants";
import { AuthGuard } from "@nestjs/passport";
import { Metadata } from "@grpc/grpc-js";

@Injectable()
export class GrpcAuthGuard extends AuthGuard("jwt") {
  constructor(private jwtService: JwtService) {
    super();
  }

  canActivate(context: ExecutionContext): boolean {
    const type = context.getType();

    let header: string;

    if (type === "rpc") {
      const metadata = context.getArgByIndex(1); // metadata

      if (!metadata) {
        throw new RpcExceptionBuilder("metadata lost", Status.UNAUTHENTICATED);
      }
      header = metadata.get("authorization").toString();
    }

    if (!header) {
      throw new RpcExceptionBuilder("header lost", Status.UNAUTHENTICATED);
    }

    let token: string;

    if (header.startsWith("Bearer ")) {
      token = header.substring(7, header.length);
    } else {
      const newMetadata: Metadata = new Metadata();
      newMetadata.set("grpc-status", "16");
      newMetadata.set("grpc-message", "token missing");
      newMetadata.set(
        "Access-Control-Expose-Headers",
        "grpc-status,grpc-message",
      );
      throw new RpcExceptionBuilder(
        "token missing",
        Status.UNAUTHENTICATED,
        newMetadata,
      );
    }

    try {
      this.jwtService.verify(token, {
        secret: process.env.JWT_SECRET || "secretKey",
      });
      return true;
    } catch (e) {
      throw new RpcExceptionBuilder("token invalid", Status.UNAUTHENTICATED);
    }
  }
}
