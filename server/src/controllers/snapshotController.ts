import type { FastifyReply, FastifyRequest } from "fastify";
import { getIslandSnapshot, upsertIslandSnapshot } from "../repositories/snapshotRepository.js";
import { validateIslandSnapshot } from "../services/snapshotValidationService.js";

interface SnapshotBody {
  snapshotJson: string;
}

export async function putSnapshot(request: FastifyRequest, reply: FastifyReply) {
  const playerId = request.auth!.playerId;
  const body = request.body as SnapshotBody;

  let parsed: Record<string, unknown>;
  try {
    parsed = JSON.parse(body.snapshotJson);
  } catch {
    return reply.code(400).send({ error: "Invalid snapshotJson" });
  }

  const validation = validateIslandSnapshot(parsed);
  if (!validation.valid) {
    return reply.code(400).send({ error: validation.reason ?? "Invalid snapshot" });
  }

  parsed.ownerPlayerId = playerId;
  await upsertIslandSnapshot(playerId, parsed);

  return reply.send({ ok: true });
}

export async function getSnapshot(request: FastifyRequest, reply: FastifyReply) {
  const { playerId } = request.params as { playerId: string };
  const row = await getIslandSnapshot(playerId);

  if (!row) {
    return reply.code(404).send({ error: "Snapshot not found" });
  }

  return reply.send({ snapshot: row.snapshot_json });
}
