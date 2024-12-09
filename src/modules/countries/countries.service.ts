import { Injectable } from "@nestjs/common";
import { PrismaService } from "src/infrastructure/database/prisma/PrismaService";
import {
  CountryList,
  CountryServiceImplementation,
} from "src/proto/country/v1/country";

@Injectable()
export class CountriesService implements CountryServiceImplementation {
  constructor(private readonly prisma: PrismaService) {}

  async getAllCountries(): Promise<CountryList> {
    const countries = await this.prisma.country.findMany();
    return { countries };
  }

  //   linkCountryToUser(userId: number): Promise<Empty> {
  //     // Привязываем страну к пользователю
  //     const updatedUser = this.prisma.user.update({
  //       where: { id: userId },
  //       data: { countryId: request.countryId },
  //     });

  //     // Возвращаем обновленный объект User
  //     return updatedUser;
  //   }
}
