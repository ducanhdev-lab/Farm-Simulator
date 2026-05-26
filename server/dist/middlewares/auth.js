import jwt from "jsonwebtoken";
import { env } from "../config/env.js";
export async function requireAuth(request, reply) {
    const header = request.headers.authorization;
    if (!header?.startsWith("Bearer ")) {
        return reply.code(401).send({ error: "Unauthorized" });
    }
    const token = header.slice("Bearer ".length);
    try {
        const payload = jwt.verify(token, env.jwtSecret);
        request.auth = payload;
    }
    catch {
        return reply.code(401).send({ error: "Invalid token" });
    }
}
