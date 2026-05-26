import type { FastifyReply, FastifyRequest } from "fastify";
import { createGuestSession } from "../services/authService.js";

export async function guestLogin(_request: FastifyRequest, reply: FastifyReply) {
  const session = await createGuestSession();
  return reply.send(session);
}
