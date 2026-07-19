using System.Collections.Generic;
using System.Linq;
using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;

namespace ICE.Scheduler;

internal static class RelicJobPicker
{
    // Priority order for "Farm all relics": crafters first, then gatherers, fisher last.
    // CrafterJobList = [8..15], GatheringJobList = [16 (MIN), 17 (BTN), 18 (FSH)], so the
    // concatenation is exactly crafters -> gatherers -> fisher-last with no extra ordering.
    private static IEnumerable<uint> PriorityOrder =>
        CosmicHelper.CrafterJobList.Concat(CosmicHelper.GatheringJobList);

    /// <summary>
    /// Picks the next job to farm a relic on, strict priority order, one relic to completion
    /// before advancing. A job is eligible when it's enabled in <see cref="Config.RelicJobs"/>,
    /// unlocked (level &gt; 0), and its relic isn't already maxed for the current hub.
    /// Returns 0 when nothing remains. <paramref name="blockedByGearset"/> distinguishes
    /// "all relics complete" (false) from "remaining relics have no gearset" (true).
    /// </summary>
    internal static uint PickNextRelicJob(out bool blockedByGearset)
    {
        var eligible = PriorityOrder
            .Where(j => C.RelicJobs.TryGetValue(j, out var enabled) && enabled
                     && Player.GetLevel((Job)j) > 0
                     && !IsRelicMaxed(j))
            .ToList();
        var withGearset = eligible
            .Where(j => GearsetHandler.HasGearset((Job)j))
            .ToList();
        blockedByGearset = withGearset.Count == 0 && eligible.Count > 0;
        return withGearset.FirstOrDefault(); // 0 when none; strict priority keeps the first
    }

    internal static bool AnyRelicJobsSelected() =>
        PriorityOrder.Any(j => C.RelicJobs.TryGetValue(j, out var enabled) && enabled);

    /// <summary>
    /// True when the job's relic tool is capped for the current hub (no pending stage upgrade
    /// and every analysis type is at its max). Mirrors the isCapped check in Task_CheckState.
    /// Returns false when relic data isn't loaded yet (empty exp) so a not-ready job is never
    /// mistaken for a completed one.
    /// </summary>
    internal static bool IsRelicMaxed(uint jobId)
    {
        var info = CosmicHelper.Cosmic_ClassInfo();
        if (!info.TryGetValue(jobId, out var relic))
            return false;

        bool pendingUpgrade = relic.Stage_Current < relic.Stage_Next;
        if (pendingUpgrade)
            return false;

        if (relic.CurrentExp.Count == 0)
            return false; // not loaded / relic not unlocked -> not "complete"

        return relic.CurrentExp.All(e => e.Value.Current == e.Value.Max);
    }
}
