import { getCoinLeaderboard } from "../controllers/leaderboardController.js";
export async function leaderboardRoutes(app) {
    app.get("/leaderboard/coins", getCoinLeaderboard);
}
