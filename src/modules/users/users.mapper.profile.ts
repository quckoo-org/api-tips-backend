import { createMap, forMember, mapFrom, Mapper } from "@automapper/core";
import { AutomapperProfile, InjectMapper } from "@automapper/nestjs";
import { UserEntity } from "./dto/user.entity";
import { UserDto } from "./dto/user.dto";
import { convertDateToTimestamp } from "src/core/shared/utils/convertDateToTimeshtamp";
import { UpdateUserDto } from "./dto/update-user.dto";

export class UserProfile extends AutomapperProfile {
  constructor(@InjectMapper() mapper: Mapper) {
    super(mapper);
  }

  override get profile() {
    return (mapper) => {
      console.log("Initializing UserProfile mapping...");
      createMap(
        mapper,
        UserEntity,
        UserDto,
        forMember(
          (destination) => destination.fistName,
          mapFrom((source) => source.name),
        ),
        forMember(
          (destination) => destination.lastName,
          mapFrom((source) => source.lastname),
        ),
        forMember(
          (destination) => destination.cca3,
          mapFrom((source) => source.countryCode),
        ),
        forMember(
          (destination) => destination.isBlocked,
          mapFrom((source) => convertDateToTimestamp(source.blockedTimestamp)),
        ),
        forMember(
          (destination) => destination.isDeleted,
          mapFrom((source) => convertDateToTimestamp(source.deletedTimestamp)),
        ),
        forMember(
          (destination) => destination.isVerified,
          mapFrom((source) => convertDateToTimestamp(source.verifiedTimestamp)),
        ),
        forMember(
          (destination) => destination.createdAt,
          mapFrom((source) => convertDateToTimestamp(source.createdAt)),
        ),
      );

      createMap(
        mapper,
        UpdateUserDto,
        UserEntity,
        forMember(
          (destination) => destination.email,
          mapFrom((source) => source.email),
        ),
        forMember(
          (destination) => destination.name,
          mapFrom((source) => source.firstName),
        ),
        forMember(
          (destination) => destination.lastname,
          mapFrom((source) => source.lastName),
        ),
        forMember(
          (destination) => destination.countryCode,
          mapFrom((source) => source.cca3),
        ),
        forMember(
          (destination) => destination.blockedTimestamp,
          mapFrom((source) => (source.isBlocked ? new Date() : null)),
        ),
        forMember(
          (destination) => destination.deletedTimestamp,
          mapFrom((source) => (source.isDeleted ? new Date() : null)),
        ),
        forMember(
          (destination) => destination.verifiedTimestamp,
          mapFrom((source) => (source.isVerified ? new Date() : null)),
        ),
      );
    };
  }
}
