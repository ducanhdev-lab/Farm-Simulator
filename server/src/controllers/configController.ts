import type { FastifyReply, FastifyRequest } from "fastify";

/** Phase 3: live balance — replace with DB/admin later. */
const balanceConfig = {
  version: 1,
  islandBasePrice: 100,
  stallMultipliers: [
    { itemId: "Tomato", multiplier: 1.1 },
    { itemId: "Carrot", multiplier: 1.05 },
    { itemId: "Corn", multiplier: 1.15 },
  ],
};

export async function getBalanceConfig(_request: FastifyRequest, reply: FastifyReply) {
  return reply.send(balanceConfig);
}
