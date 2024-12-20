import {
  createMap,
  forMember,
  ignore,
  mapFrom,
  Mapper,
} from "@automapper/core";
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
          (destination) => destination.id,
          mapFrom((source) => source.id),
        ),
        forMember(
          (destination) => destination.email,
          mapFrom((source) => source.email),
        ),
        forMember(
          (destination) => destination.firstName,
          mapFrom((source) => source.firstName),
        ),
        forMember(
          (destination) => destination.lastName,
          mapFrom((source) => source.lastName),
        ),
        forMember(
          (destination) => destination.countryCode,
          mapFrom((source) => source.countryCode),
        ),
        forMember(
          (destination) => destination.blockedAt,
          mapFrom((source) => convertDateToTimestamp(source.blockedAt)),
        ),
        forMember(
          (destination) => destination.deletedAt,
          mapFrom((source) => convertDateToTimestamp(source.deletedAt)),
        ),
        forMember(
          (destination) => destination.verifiedAt,
          mapFrom((source) => convertDateToTimestamp(source.verifiedAt)),
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
          (destination) => destination.firstName,
          mapFrom((source) => source.firstName),
        ),
        forMember(
          (destination) => destination.lastName,
          mapFrom((source) => source.lastName),
        ),
        forMember(
          (destination) => destination.countryCode,
          mapFrom((source) => source.countryCode),
        ),
        forMember(
          (destination) => destination.blockedAt,
          mapFrom((source) => {
            // (source.isBlocked ? new Date() : undefined)

            if (source.isBlocked === undefined) {
              return undefined;
            }

            if (source.isBlocked) {
              return new Date();
            } else {
              return null;
            }
          }),
        ),
        forMember(
          (destination) => destination.deletedAt,
          mapFrom((source) => {
            //   (source.isDeleted ? new Date() : undefined)
            if (source.isDeleted === undefined) {
              return undefined;
            }

            if (source.isDeleted) {
              return new Date();
            } else {
              return null;
            }
          }),
        ),
        forMember(
          (destination) => destination.verifiedAt,
          mapFrom((source) => {
            // (source.isVerified ? new Date() : undefined)

            if (source.isVerified === undefined) {
              return undefined;
            }

            if (source.isVerified) {
              return new Date();
            } else {
              return null;
            }
          }),
        ),
      );
    };
  }
}
