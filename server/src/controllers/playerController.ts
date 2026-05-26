import type { FastifyReply, FastifyRequest } from "fastify";
import { getPlayerProfile, upsertPlayerProfile } from "../repositories/playerRepository.js";
import { upsertLeaderboardScore } from "../repositories/leaderboardRepository.js";
import { validatePlayerProfile } from "../services/saveValidationService.js";

interface SaveBody {
  saveVersion: number;
  profileJson: string;
}

export async function getProfile(request: FastifyRequest, reply: FastifyReply) {
  const playerId = request.auth!.playerId;
  const row = await getPlayerProfile(playerId);

  if (!row) {
    return reply.send({});
  }

  return reply.send(row.profile_json);
}

export async function putSave(request: FastifyRequest, reply: FastifyReply) {
  const playerId = request.auth!.playerId;
  const body = request.body as SaveBody;

  let parsed: Record<string, unknown>;
  try {
    parsed = JSON.parse(body.profileJson);
  } catch {
    return reply.code(400).send({ error: "Invalid profileJson" });
  }

  const existing = await getPlayerProfile(playerId);
  const validation = validatePlayerProfile(parsed, {
    previousProfile: existing?.profile_json ?? null,
    previousUpdatedAt: existing?.updated_at ? new Date(existing.updated_at) : null,
  });
  if (!validation.valid) {
    return reply.code(400).send({ error: validation.reason ?? "Invalid profile" });
  }

  const result = await upsertPlayerProfile(playerId, body.saveVersion ?? 1, parsed);

  if (result.conflict) {
    return reply.code(409).send({ error: "Save version conflict" });
  }

  if (validation.coins != null) {
    await upsertLeaderboardScore(playerId, validation.coins, body.saveVersion ?? 1);
  }

  return reply.send({ ok: true });
}
