import { guestLogin } from "../controllers/authController.js";
export async function authRoutes(app) {
    app.post("/auth/guest", guestLogin);
}
