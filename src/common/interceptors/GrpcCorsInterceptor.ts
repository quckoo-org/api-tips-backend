import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
} from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Metadata } from '@grpc/grpc-js';
import { log } from '@grpc/grpc-js/build/src/logging';

@Injectable()
export class GrpcCorsInterceptor implements NestInterceptor {
  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const rpcContext = context.switchToRpc();
    const call = rpcContext.getContext(); // gRPC контекст
    console.log(call);
    if (call && typeof call.sendMetadata === 'function') {
      const metadata = new Metadata();
      metadata.set(
        'Access-Control-Expose-Headers',
        'grpc-status, grpc-message',
      );
      console.log('wwork');
      call.sendMetadata(metadata);
    }

    return next.handle().pipe(
      tap({
        error: (err) => {
          if (call && typeof call.sendMetadata === 'function') {
            const errorMetadata = new Metadata();
            errorMetadata.set('grpc-status', '13'); // INTERNAL error
            errorMetadata.set('grpc-message', err.message || 'Unknown error');
            call.sendMetadata(errorMetadata);
          }
        },
      }),
    );
  }
}
