import { getProfile, putSave } from "../controllers/playerController.js";
import { requireAuth } from "../middlewares/auth.js";
import { rateLimitSave } from "../middlewares/rateLimit.js";
export async function playerRoutes(app) {
    app.addHook("preHandler", requireAuth);
    app.get("/player/profile", getProfile);
    app.put("/player/save", { preHandler: rateLimitSave }, putSave);
}
