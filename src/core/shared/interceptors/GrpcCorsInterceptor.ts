import {
  CallHandler,
  ExecutionContext,
  Injectable,
  NestInterceptor,
} from "@nestjs/common";
import { Observable } from "rxjs";
import { Metadata } from "@grpc/grpc-js";

@Injectable()
export class GrpcCorsInterceptor implements NestInterceptor {
  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const meta = context.switchToRpc().getContext<Metadata>();

    // You can modify, add and remove metadata here in the interceptor which
    // The metadata will be passed to the next handler(ProductController)
    meta.set("Access-Control-Expose-Headers", "grpc-status,grpc-message");
    meta.set("Access-Control-Allow-Origin", "*");
    meta.set("Access-Control-Allow-Credentials", "true");
    meta.set(
      "Access-Control-Allow-Headers",
      "keep-alive,user-agent,cache-control,content-type,content-transfer-encoding,custom-header-1,x-accept-content-transfer-encoding,x-accept-response-streaming,x-user-agent,x-grpc-web,grpc-timeout, authorization",
    );
    meta.set("Access-Control-Allow-Methods", "*");
    // meta.set('grpc-status', '0');

    return next.handle();
  }
}
