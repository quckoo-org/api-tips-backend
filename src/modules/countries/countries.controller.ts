import { Metadata, ServerUnaryCall } from "@grpc/grpc-js";
import { Injectable } from "@nestjs/common";
import {
  CountryList,
  CountryServiceController,
  CountryServiceControllerMethods,
  Empty,
} from "src/proto/country/v1/country";
import { CountriesService } from "./countries.service";

@Injectable()
@CountryServiceControllerMethods()
export class CountriesController implements CountryServiceController {
  constructor(private readonly countriesService: CountriesService) {}

  async getAllCountries(
    _: Empty,
    metadata: Metadata,
    call: ServerUnaryCall<any, any>,
  ): Promise<CountryList> {
    call.sendMetadata(metadata);
    const countries = await this.countriesService.getAllCountries();
    return countries;
  }
}
