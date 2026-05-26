import { getIslandSnapshot, upsertIslandSnapshot } from "../repositories/snapshotRepository.js";
import { validateIslandSnapshot } from "../services/snapshotValidationService.js";
export async function putSnapshot(request, reply) {
    const playerId = request.auth.playerId;
    const body = request.body;
    let parsed;
    try {
        parsed = JSON.parse(body.snapshotJson);
    }
    catch {
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
export async function getSnapshot(request, reply) {
    const { playerId } = request.params;
    const row = await getIslandSnapshot(playerId);
    if (!row) {
        return reply.code(404).send({ error: "Snapshot not found" });
    }
    return reply.send({ snapshot: row.snapshot_json });
}
