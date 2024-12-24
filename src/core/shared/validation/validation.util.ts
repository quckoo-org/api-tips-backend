import { plainToInstance } from "class-transformer";
import { validate } from "class-validator";
import { ValidationError } from "class-validator/types/validation/ValidationError";

export class ValidationUtil {
  static async validateAndThrow<T extends object>(
    dtoClass: new () => T,
    rawObject: object,
  ): Promise<T> {
    const dtoInstance = plainToInstance(dtoClass, rawObject);

    // const errors = await validate(dtoInstance);
    // console.log(errors, "eerrors");
    // if (errors.length > 0) {
    //   const errorMessage = errors
    //     .map((err) => Object.values(err.constraints || {}).join(", "))
    //     .join("; ");
    //   console.log(errorMessage, "errorMessage");
    //   throw new RpcExceptionBuilder(errorMessage, Status.INVALID_ARGUMENT);
    // }

    return dtoInstance;
  }

  static async validate<T extends object>(
    dtoClass: new () => T,
    rawObject: object,
  ): Promise<ValidationError[]> {
    const dtoInstance = plainToInstance(dtoClass, rawObject);

    const errors = await validate(dtoInstance);

    return errors;
  }
}
