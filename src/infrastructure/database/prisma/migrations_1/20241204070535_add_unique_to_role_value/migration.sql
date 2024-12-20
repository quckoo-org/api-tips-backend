/*
  Warnings:

  - A unique constraint covering the columns `[value]` on the table `Role` will be added. If there are existing duplicate values, this will fail.

*/
-- CreateIndex
CREATE UNIQUE INDEX "Role_value_key" ON "Role"("value");
