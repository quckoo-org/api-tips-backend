import { Timestamp } from "src/proto/google/protobuf/timestamp";

export function convertDateToTimestamp(date: Date | null): Timestamp {
  if (!date) {
    return null;
  }
  const seconds = Math.floor(date.getTime() / 1000);
  const nanos = (date.getTime() % 1000) * 1e6;
  return { seconds, nanos };
}
