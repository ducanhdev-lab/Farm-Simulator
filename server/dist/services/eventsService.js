import { getActiveEventsFromDb } from "../repositories/eventsRepository.js";
const fallbackCatalog = [
    {
        eventId: "event_snow",
        sceneName: "Level02",
        displayName: "Snow Festival",
        description: "Limited snow biome rewards.",
        alwaysAvailable: true,
        requiredHomeCoins: 0,
        startsAt: null,
        endsAt: null,
    },
    {
        eventId: "event_coast",
        sceneName: "Level03",
        displayName: "Fishing Coast",
        description: "Catch rare fish for bonus coins.",
        alwaysAvailable: false,
        requiredHomeCoins: 2000,
        startsAt: null,
        endsAt: null,
    },
];
function mapRow(row) {
    return {
        eventId: row.event_id,
        sceneName: row.scene_name,
        displayName: row.display_name,
        description: row.description,
        alwaysAvailable: row.always_available,
        requiredHomeCoins: row.required_home_coins,
        startsAt: row.starts_at,
        endsAt: row.ends_at,
    };
}
export async function getActiveEvents(now = new Date()) {
    try {
        const rows = await getActiveEventsFromDb(now);
        if (rows.length > 0) {
            return rows.map(mapRow);
        }
    }
    catch {
        // DB unavailable — use static catalog for local dev without postgres tables.
    }
    return fallbackCatalog.filter((event) => {
        if (event.startsAt) {
            const start = new Date(event.startsAt);
            if (now < start)
                return false;
        }
        if (event.endsAt) {
            const end = new Date(event.endsAt);
            if (now > end)
                return false;
        }
        return true;
    });
}
