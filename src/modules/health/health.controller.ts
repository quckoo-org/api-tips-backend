import { Controller } from "@nestjs/common";
import { GrpcMethod } from "@nestjs/microservices";
import {
  HealthCheckRequest,
  HealthCheckResponse,
  HealthCheckResponse_ServingStatus,
} from "../../proto/health/v1/health";

@Controller()
export class HealthController {
  @GrpcMethod("Health", "Check")
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  check(data: HealthCheckRequest, metadata: any): HealthCheckResponse {
    return { status: HealthCheckResponse_ServingStatus.SERVING };
  }
}
