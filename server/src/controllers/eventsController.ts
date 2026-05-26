import type { FastifyReply, FastifyRequest } from "fastify";
import { getActiveEvents } from "../services/eventsService.js";

export async function getActiveEventsHandler(_request: FastifyRequest, reply: FastifyReply) {
  const events = await getActiveEvents();
  return reply.send({ events });
}
