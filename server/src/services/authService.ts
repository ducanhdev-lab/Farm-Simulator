import jwt from "jsonwebtoken";
import { v4 as uuid } from "uuid";
import { env } from "../config/env.js";
import { createPlayer } from "../repositories/playerRepository.js";

export async function createGuestSession() {
  const playerId = uuid();
  await createPlayer(playerId);
  const token = jwt.sign({ playerId }, env.jwtSecret, { expiresIn: "30d" });
  return { token, playerId };
}
