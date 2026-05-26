import { getPlayerProfile, upsertPlayerProfile } from "../repositories/playerRepository.js";
export async function getProfile(request, reply) {
    const playerId = request.auth.playerId;
    const row = await getPlayerProfile(playerId);
    if (!row) {
        return reply.send({});
    }
    return reply.send(row.profile_json);
}
export async function putSave(request, reply) {
    const playerId = request.auth.playerId;
    const body = request.body;
    let parsed;
    try {
        parsed = JSON.parse(body.profileJson);
    }
    catch {
        return reply.code(400).send({ error: "Invalid profileJson" });
    }
    const result = await upsertPlayerProfile(playerId, body.saveVersion ?? 1, parsed);
    if (result.conflict) {
        return reply.code(409).send({ error: "Save version conflict" });
    }
    return reply.send({ ok: true });
}
