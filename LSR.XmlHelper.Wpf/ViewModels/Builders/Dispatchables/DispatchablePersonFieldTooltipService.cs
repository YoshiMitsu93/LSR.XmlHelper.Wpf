using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public static class DispatchablePersonFieldTooltipService
    {
        private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DebugName"] = "Internal identifier for this dispatchable person entry. Used as the lookup key when editing/replacing entries.",
            ["ModelName"] = "Ped model to spawn (e.g. s_m_y_cop_01). This controls the actual ped appearance archetype.",
            ["GroupName"] = "Group ID this person belongs to. LSR uses this to keep entries tied to the DispatchablePersonGroupID.",
            ["AmbientSpawnChance"] = "Spawn chance used when Wanted Level is 0. If 0, this person will not spawn for ambient situations.",
            ["WantedSpawnChance"] = "Spawn chance used when Wanted Level is > 0 and within Min/Max Wanted range. If 0, cannot spawn in wanted situations.",
            ["MinWantedLevelSpawn"] = "Minimum wanted level required for this person to be eligible in wanted spawns.",
            ["MaxWantedLevelSpawn"] = "Maximum wanted level allowed for this person to be eligible in wanted spawns.",
            ["HealthMin"] = "Minimum health applied to the spawned ped (randomized within min/max).",
            ["HealthMax"] = "Maximum health applied to the spawned ped (randomized within min/max).",
            ["ArmorMin"] = "Minimum armor applied to the spawned ped (randomized within min/max).",
            ["ArmorMax"] = "Maximum armor applied to the spawned ped (randomized within min/max).",
            ["AccuracyMin"] = "Minimum combat accuracy used for the spawned ped (randomized within min/max).",
            ["AccuracyMax"] = "Maximum combat accuracy used for the spawned ped (randomized within min/max).",
            ["ShootRateMin"] = "Minimum shoot rate used for the spawned ped (randomized within min/max).",
            ["ShootRateMax"] = "Maximum shoot rate used for the spawned ped (randomized within min/max).",
            ["CombatAbilityMin"] = "Minimum combat ability used for the spawned ped (randomized within min/max). 0=poor, 1=average, 2=professional.",
            ["CombatAbilityMax"] = "Maximum combat ability used for the spawned ped (randomized within min/max). 0=poor, 1=average, 2=professional.",
            ["CombatRange"] = "Combat range passed directly to GTA native SET_PED_COMBAT_RANGE. -1 = LSR does not force a value. 0=Near, 1=Medium, 2=Far, 3=Very Far.",
            ["CombatMovement"] = "Combat movement passed directly to GTA native SET_PED_COMBAT_MOVEMENT. -1 = LSR does not force a value. 0=Stationary, 1=Defensive (cover-first), 2=Offensive (pushes), 3=Suicidal Offensive (very aggressive flanks).",
            ["TaserAccuracyMin"] = "Minimum accuracy when using taser (randomized within min/max).",
            ["TaserAccuracyMax"] = "Maximum accuracy when using taser (randomized within min/max).",
            ["TaserShootRateMin"] = "Minimum shoot rate when using taser (randomized within min/max).",
            ["TaserShootRateMax"] = "Maximum shoot rate when using taser (randomized within min/max).",
            ["VehicleAccuracyMin"] = "Minimum accuracy while shooting from a vehicle (randomized within min/max).",
            ["VehicleAccuracyMax"] = "Maximum accuracy while shooting from a vehicle (randomized within min/max).",
            ["VehicleShootRateMin"] = "Minimum shoot rate while shooting from a vehicle (randomized within min/max).",
            ["VehicleShootRateMax"] = "Maximum shoot rate while shooting from a vehicle (randomized within min/max).",
            ["TurretAccuracyMin"] = "Minimum accuracy while using a turret/vehicle mounted weapon (randomized within min/max).",
            ["TurretAccuracyMax"] = "Maximum accuracy while using a turret/vehicle mounted weapon (randomized within min/max).",
            ["TurretShootRateMin"] = "Minimum shoot rate while using a turret/vehicle mounted weapon (randomized within min/max).",
            ["TurretShootRateMax"] = "Maximum shoot rate while using a turret/vehicle mounted weapon (randomized within min/max).",
            ["UnitCode"] = "Optional unit code label used by LSR for some dispatch/AI naming logic.",
            ["RequiredHelmetType"] = "Helmet requirement preset. Used when LSR decides helmet behavior for this ped.",
            ["AllowRandomizeBeforeVariationApplied"] = "If true, LSR allows randomization before applying RequiredVariation.",
            ["RandomizeHead"] = "If true, LSR may randomize head/face data (mainly for freemode models or where supported).",
            ["OverrideAgencyLessLethalWeapons"] = "If true, uses override less-lethal weapon list/ID instead of the agency default.",
            ["OverrideAgencySideArms"] = "If true, uses override sidearm list/ID instead of the agency default.",
            ["OverrideAgencyLongGuns"] = "If true, uses override long gun list/ID instead of the agency default.",
            ["OverrideLessLethalWeaponsID"] = "IssuableWeapons group ID used for less-lethal overrides (when enabled).",
            ["OverrideSideArmsID"] = "IssuableWeapons group ID used for sidearm overrides (when enabled).",
            ["OverrideLongGunsID"] = "IssuableWeapons group ID used for long gun overrides (when enabled).",
            ["OverrideSideArms"] = "Explicit list of sidearms to use instead of the agency defaults (when enabled).",
            ["OverrideLongGuns"] = "Explicit list of long guns to use instead of the agency defaults (when enabled).",
            ["OverrideLessLethalWeapons"] = "Explicit list of less-lethal weapons to use instead of the agency defaults (when enabled).",
            ["RequiredVariation"] = "Fixed ped clothing/variation configuration that LSR applies to the ped.",
            ["OptionalProps"] = "Optional prop set that can be applied to the ped based on OptionalPropChance.",
            ["OptionalPropChance"] = "Chance that optional props will be applied when spawning this ped.",
            ["OptionalComponents"] = "Optional clothing/components list that can be applied to the ped based on OptionalComponentChance.",
            ["OptionalAppliedOverlayLogic"] = "Overlay/decals logic used when applying optional variation data.",
            ["OptionalComponentChance"] = "Chance that optional components will be applied when spawning this ped.",
            ["EmptyHolster"] = "Holster drawable/variation state used when the holster should appear empty.",
            ["FullHolster"] = "Holster drawable/variation state used when the holster should appear full.",
            ["OverrideVoice"] = "List of voice names; LSR picks one at random and assigns it to the ped.",
            ["CustomPropAttachments"] = "Attachment setup for props. (May be unused).",
            ["DisableWrithe"] = "If true, disables writhe behavior for this ped when injured.",
            ["DisableWritheShooting"] = "If true, disables shooting while writhing for this ped.",
            ["DisableCriticalHits"] = "If true, LSR disables critical hits for this ped.",
            ["DisableBulletRagdoll"] = "If true, prevents ragdoll from bullet impact for this ped (ped config flag).",
            ["HasFullBodyArmor"] = "If true, LSR treats the ped as having full body armor behavior.",
            ["FiringPatternHash"] = "Firing pattern hash used for ped combat behavior. Controls burst/aiming pattern.",
            ["PedConfigFlagsToSet"] = "List of ped config flags that LSR applies to the ped on spawn.",
            ["CombatAttributesToSet"] = "List of combat attributes that LSR applies to the ped on spawn.",
            ["CombatFloatsToSet"] = "List of combat float settings that LSR applies to the ped on spawn.",
            ["FaceFeatureRandomizePercentage"] = "Chance to randomize face features. (May be unused).",
            ["AlwaysHasLongGun"] = "If true, LSR sets pedExt.AlwaysHasLongGun so the ped will always be equipped with a long gun when appropriate.",
            ["IsAnimal"] = "If true, marks this dispatchable as an animal entry for LSR handling.",
            ["OverrideSightDistance"] = "If set, LSR overrides the ped sight distance (how far they can detect targets).",
            ["OverrideHelmet"] = "If true, forces helmet override behavior for this ped.",
            ["NoHelmetPercentage"] = "Percentage chance that the ped spawns without a helmet when helmet logic applies.",
            ["ShrinkHeadForMask"] = "If true, shrinks head for mask compatibility when applying variations.",
            ["IsInvisibleAndInvulnerable"] = "If true, spawns as invisible and invulnerable (used for special effects/logic entries)."
        };

        public static string GetTooltip(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return "";

            if (Map.TryGetValue(fieldName, out var tooltip))
                return tooltip;

            return "No tooltip is defined for this field in the helper yet. It may be unused or only used in very specific situations.";
        }
    }
}
