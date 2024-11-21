import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
} from '@nestjs/common';
import { Observable } from 'rxjs';

@Injectable()
export class GrpcCorsInterceptor implements NestInterceptor {
  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const call = context.switchToRpc().getContext();

    if (call.metadata) {
      const headers = {
        'access-control-allow-origin': '*',
        'access-control-allow-methods': 'GET,POST,OPTIONS',
        'access-control-allow-headers': 'Content-Type, Accept, Authorization',
      };

      Object.keys(headers).forEach((key) => {
        call.metadata.add(key, headers[key]);
      });
    }

    return next.handle();
  }
}
