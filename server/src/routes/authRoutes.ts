import type { FastifyInstance } from "fastify";
import { guestLogin } from "../controllers/authController.js";

export async function authRoutes(app: FastifyInstance) {
  app.post("/auth/guest", guestLogin);
}
