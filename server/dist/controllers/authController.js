import { createGuestSession } from "../services/authService.js";
export async function guestLogin(_request, reply) {
    const session = await createGuestSession();
    return reply.send(session);
}
