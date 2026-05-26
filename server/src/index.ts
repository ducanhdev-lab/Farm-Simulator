import Fastify from "fastify";
import cors from "@fastify/cors";
import { env } from "./config/env.js";
import { initDatabase } from "./config/database.js";
import { authRoutes } from "./routes/authRoutes.js";
import { playerRoutes } from "./routes/playerRoutes.js";

const app = Fastify({ logger: true });

await app.register(cors, { origin: true });
await initDatabase();

app.get("/health", async () => ({ status: "ok" }));

await app.register(authRoutes);
await app.register(playerRoutes);

app.listen({ port: env.port, host: "0.0.0.0" });
