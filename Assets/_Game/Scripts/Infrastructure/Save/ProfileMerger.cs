using System;
using System.Collections.Generic;
using System.Linq;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Offline-first merge: higher saveVersion wins; ties use lastSaveUtc.
    /// </summary>
    public static class ProfileMerger
    {
        public static PlayerProfile Merge(PlayerProfile local, PlayerProfile remote)
        {
            if (local == null)
                return remote;
            if (remote == null)
                return local;

            if (IsRemoteNewer(local, remote))
                return MergePreferRemote(local, remote);

            if (IsLocalNewer(local, remote))
                return MergePreferLocal(local, remote);

            return MergePreferLocal(local, remote);
        }

        public static bool IsRemoteNewer(PlayerProfile local, PlayerProfile remote)
        {
            if (remote == null)
                return false;
            if (local == null)
                return true;

            if (remote.saveVersion != local.saveVersion)
                return remote.saveVersion > local.saveVersion;

            return CompareUtc(remote.lastSaveUtc, local.lastSaveUtc) > 0;
        }

        private static bool IsLocalNewer(PlayerProfile local, PlayerProfile remote) =>
            IsRemoteNewer(remote, local);

        private static PlayerProfile MergePreferRemote(PlayerProfile local, PlayerProfile remote)
        {
            var merged = CloneShell(remote);
            merged.homeWorld = remote.homeWorld ?? local.homeWorld;
            merged.cosmetics = MergeCosmetics(local.cosmetics, remote.cosmetics, preferRemote: true);
            merged.eventInstances = MergeEventInstances(local.eventInstances, remote.eventInstances);
            merged.saveVersion = Math.Max(local.saveVersion, remote.saveVersion);
            merged.lastSaveUtc = PickNewerUtc(local.lastSaveUtc, remote.lastSaveUtc);
            ProfileMigration.SyncCosmeticsToHomeWorld(merged);
            return merged;
        }

        private static PlayerProfile MergePreferLocal(PlayerProfile local, PlayerProfile remote)
        {
            var merged = CloneShell(local);
            merged.homeWorld = local.homeWorld ?? remote.homeWorld;
            merged.cosmetics = MergeCosmetics(local.cosmetics, remote.cosmetics, preferRemote: false);
            merged.eventInstances = MergeEventInstances(local.eventInstances, remote.eventInstances);
            merged.saveVersion = Math.Max(local.saveVersion, remote.saveVersion);
            merged.lastSaveUtc = PickNewerUtc(local.lastSaveUtc, remote.lastSaveUtc);
            ProfileMigration.SyncCosmeticsToHomeWorld(merged);
            return merged;
        }

        private static PlayerProfile CloneShell(PlayerProfile source) =>
            new PlayerProfile
            {
                profileId = source.profileId,
                saveVersion = source.saveVersion,
                lastSaveUtc = source.lastSaveUtc,
                cosmetics = source.cosmetics
            };

        private static CosmeticsData MergeCosmetics(
            CosmeticsData local,
            CosmeticsData remote,
            bool preferRemote)
        {
            local ??= new CosmeticsData();
            remote ??= new CosmeticsData();

            var merged = new CosmeticsData
            {
                pendingCoins = Math.Max(local.pendingCoins, remote.pendingCoins),
                equippedSkinId = preferRemote
                    ? FirstNonEmpty(remote.equippedSkinId, local.equippedSkinId)
                    : FirstNonEmpty(local.equippedSkinId, remote.equippedSkinId)
            };

            merged.ownedSkinIds = local.ownedSkinIds
                .Concat(remote.ownedSkinIds ?? new List<string>())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            merged.completedIAPProductIds = local.completedIAPProductIds
                .Concat(remote.completedIAPProductIds ?? new List<string>())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            return merged;
        }

        private static List<EventInstanceData> MergeEventInstances(
            List<EventInstanceData> local,
            List<EventInstanceData> remote)
        {
            var map = new Dictionary<string, EventInstanceData>();

            foreach (var instance in local ?? new List<EventInstanceData>())
            {
                if (!string.IsNullOrEmpty(instance?.eventId))
                    map[instance.eventId] = instance;
            }

            foreach (var instance in remote ?? new List<EventInstanceData>())
            {
                if (string.IsNullOrEmpty(instance?.eventId))
                    continue;

                if (!map.TryGetValue(instance.eventId, out var existing))
                {
                    map[instance.eventId] = instance;
                    continue;
                }

                map[instance.eventId] = CompareUtc(instance.lastPlayedUtc, existing.lastPlayedUtc) >= 0
                    ? instance
                    : existing;
            }

            return map.Values.ToList();
        }

        private static string FirstNonEmpty(string a, string b) =>
            !string.IsNullOrEmpty(a) ? a : b;

        private static string PickNewerUtc(string a, string b) =>
            CompareUtc(a, b) >= 0 ? (string.IsNullOrEmpty(a) ? b : a) : b;

        private static int CompareUtc(string a, string b)
        {
            if (!DateTime.TryParse(a, out var da))
                da = DateTime.MinValue;
            if (!DateTime.TryParse(b, out var db))
                db = DateTime.MinValue;
            return da.CompareTo(db);
        }
    }
}
