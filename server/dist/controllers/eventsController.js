import { getActiveEvents } from "../services/eventsService.js";
export async function getActiveEventsHandler(_request, reply) {
    const events = await getActiveEvents();
    return reply.send({ events });
}
