import type { FastifyInstance } from "fastify";
import { getActiveEventsHandler } from "../controllers/eventsController.js";

export async function eventsRoutes(app: FastifyInstance) {
  app.get("/events/active", getActiveEventsHandler);
}
