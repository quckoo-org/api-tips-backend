/*
  Warnings:

  - Added the required column `password` to the `User` table without a default value. This is not possible if the table is not empty.

*/
-- AlterTable
ALTER TABLE "User" ADD COLUMN     "blockedTimestamp" TIMESTAMP(3),
ADD COLUMN     "countryCode" TEXT,
ADD COLUMN     "deletedTimestamp" TIMESTAMP(3),
ADD COLUMN     "lastname" TEXT,
ADD COLUMN     "password" TEXT NOT NULL,
ADD COLUMN     "verifiedTimestamp" TIMESTAMP(3);
