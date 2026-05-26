import type { FastifyReply, FastifyRequest } from "fastify";
import jwt from "jsonwebtoken";
import { env } from "../config/env.js";

export interface AuthPayload {
  playerId: string;
}

declare module "fastify" {
  interface FastifyRequest {
    auth?: AuthPayload;
  }
}

export async function requireAuth(request: FastifyRequest, reply: FastifyReply) {
  const header = request.headers.authorization;
  if (!header?.startsWith("Bearer ")) {
    return reply.code(401).send({ error: "Unauthorized" });
  }

  const token = header.slice("Bearer ".length);
  try {
    const payload = jwt.verify(token, env.jwtSecret) as AuthPayload;
    request.auth = payload;
  } catch {
    return reply.code(401).send({ error: "Invalid token" });
  }
}
