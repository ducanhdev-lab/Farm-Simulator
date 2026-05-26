import type { FastifyInstance } from "fastify";
import { getBalanceConfig } from "../controllers/configController.js";

export async function configRoutes(app: FastifyInstance) {
  app.get("/config/balance", getBalanceConfig);

  app.get("/config/balance/", async (_req, reply) => {
    return reply.redirect("/config/balance");
  });
}
