import { Controller } from '@nestjs/common';
import {GrpcMethod} from "@nestjs/microservices";
import {TestService} from "./test.service";

@Controller('test')
export class TestController {
	constructor(private readonly testService: TestService) {}

	@GrpcMethod('TestService', 'PingPong') // Указываем имя сервиса и метода из .proto
	pingPong(data: { ping: string }): { pong: string } {
		return this.testService.pingPong(data);
	}
}
