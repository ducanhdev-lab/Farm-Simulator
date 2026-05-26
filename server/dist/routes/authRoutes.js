import { guestLogin } from "../controllers/authController.js";
export async function authRoutes(app) {
    app.get("/auth/guest", async (_request, reply) => reply.send({
        message: "Use POST with an empty JSON body: {}",
        example: "curl -X POST http://localhost:3000/auth/guest -H \"Content-Type: application/json\" -d \"{}\"",
    }));
    app.post("/auth/guest", guestLogin);
}
