import type { FastifyInstance } from "fastify";
import { getSnapshot, putSnapshot } from "../controllers/snapshotController.js";
import { requireAuth } from "../middlewares/auth.js";

export async function snapshotRoutes(app: FastifyInstance) {
  app.addHook("preHandler", requireAuth);

  app.put("/island/snapshot", putSnapshot);
  app.get("/island/snapshot/:playerId", getSnapshot);
}
