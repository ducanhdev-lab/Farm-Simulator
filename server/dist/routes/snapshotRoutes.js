import { getSnapshot, putSnapshot } from "../controllers/snapshotController.js";
import { requireAuth } from "../middlewares/auth.js";
export async function snapshotRoutes(app) {
    app.addHook("preHandler", requireAuth);
    app.put("/island/snapshot", putSnapshot);
    app.get("/island/snapshot/:playerId", getSnapshot);
}
