import { getActiveEventsHandler } from "../controllers/eventsController.js";
export async function eventsRoutes(app) {
    app.get("/events/active", getActiveEventsHandler);
}
