import { Catch, Logger } from '@nestjs/common';
import { BaseRpcExceptionFilter, RpcException } from '@nestjs/microservices';
import { StatusBuilder, StatusObject } from '@grpc/grpc-js';
import { Observable, throwError } from 'rxjs';

@Catch(RpcException)
export class RpcValidationExceptionFilter extends BaseRpcExceptionFilter {
  isError(exception: any): exception is Error {
    const logger = new Logger();
    logger.error(exception);
    return super.isError(exception);
  }

  catch(exception: RpcException): Observable<StatusObject> {
    const error = exception.getError() as StatusObject;

    const statusBuilder = new StatusBuilder();
    const statusObject = statusBuilder
      .withCode(error.code)
      .withDetails(error.details)
      .withMetadata(error.metadata)
      .build();
    console.log(statusObject);

    return throwError(() => statusObject);
  }
}
