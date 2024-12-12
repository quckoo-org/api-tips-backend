import { Controller } from "@nestjs/common";

@Controller()
export class HealthController {
  // @GrpcMethod("Health", "Check")
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  // check(data: HealthCheckRequest, metadata: any): HealthCheckResponse {
  //   return { status: HealthCheckResponse_ServingStatus.SERVING };
  // }
}
