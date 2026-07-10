using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;

namespace ICE.Scheduler;

internal static class CrafterLevelingPicker
{
    internal static uint PickNextCrafter(out bool blockedByGearset)
    {
        var notDone = CosmicHelper.CrafterJobList
            .Where(j => C.LevelAllCrafterJobs.Contains(j) && Player.GetLevel((Job)j) < C.TargetLevel)
            .ToList();
        var withGearset = notDone
            .Where(j => GearsetHandler.HasGearset((Job)j))
            .OrderBy(j => Player.GetLevel((Job)j))
            .ToList();
        blockedByGearset = withGearset.Count == 0 && notDone.Count > 0;
        return withGearset.FirstOrDefault(); // 0 when none
    }

    internal static bool AnyCraftersSelected() => C.LevelAllCrafterJobs.Any(j => CosmicHelper.CrafterJobList.Contains(j));
}
