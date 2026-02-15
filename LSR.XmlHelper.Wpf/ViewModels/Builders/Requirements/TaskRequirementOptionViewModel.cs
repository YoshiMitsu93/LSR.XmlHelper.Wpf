using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class TaskRequirementOptionViewModel
    {
        public TaskRequirementOptionViewModel(string text, string description)
        {
            Text = text;
            Description = description;
        }

        public string Text { get; }
        public string Description { get; }

        public static IReadOnlyList<TaskRequirementOptionViewModel> CreateDefaults()
        {
            return new[]
            {
                new TaskRequirementOptionViewModel(
                    "None",
                    "Default ambient behavior. If spawned from a location, they are allowed to guard. If not, they may patrol normally."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard",
                    "Forces the ped to guard the area. The ped will stay at their spawn position instead of walking away."
                ),
                new TaskRequirementOptionViewModel(
                    "Patrol",
                    "Forces the ped to patrol on foot. They will walk around instead of standing in one place."
                ),
                new TaskRequirementOptionViewModel(
                    "StandardScenario",
                    "Uses the standard scenario pool. While guarding, the ped plays normal scenario idles (standing, smoking, leaning)."
                ),
                new TaskRequirementOptionViewModel(
                    "AnyScenario",
                    "Like StandardScenario but uses a much larger pool. More variety, but less controlled."
                ),
                new TaskRequirementOptionViewModel(
                    "LocalScenario",
                    "Uses nearby map scenario nodes (benches, walls, ATMs, built-in interactions)."
                ),
                new TaskRequirementOptionViewModel(
                    "BasicScenario",
                    "A small simple set of idle animations. Overrides StandardScenario and AnyScenario if used together."
                ),
                new TaskRequirementOptionViewModel(
                    "EquipLongGunWhenIdle",
                    "While idle, the ped equips their long gun and will not play normal scenarios."
                ),
                new TaskRequirementOptionViewModel(
                    "EquipSidearmWhenIdle",
                    "While idle, the ped equips their sidearm and will not play normal scenarios."
                ),
                new TaskRequirementOptionViewModel(
                    "EquipMeleeWhenIdle",
                    "While idle, the ped equips a melee weapon and will not play normal scenarios."
                ),
                new TaskRequirementOptionViewModel(
                    "CanMoveWhenGuarding",
                    "Only applies with Equip*WhenIdle options. Allows slight movement while guarding instead of being perfectly stationary."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard StandardScenario",
                    "Static gang member, casual behavior."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard AnyScenario",
                    "Static gang member with more animation variety."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard LocalScenario",
                    "Static gang member using nearby world props (walls, benches, etc.)."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard BasicScenario",
                    "Simple guard that does not do much."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard EquipLongGunWhenIdle",
                    "Armed gang member standing guard with a long gun. No scenario idles."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard EquipLongGunWhenIdle CanMoveWhenGuarding",
                    "Armed long-gun guard who can reposition slightly."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard EquipSidearmWhenIdle",
                    "Armed guard with a sidearm (police-style). No scenario idles."
                ),
                new TaskRequirementOptionViewModel(
                    "Guard EquipSidearmWhenIdle CanMoveWhenGuarding",
                    "Armed sidearm guard who can reposition slightly."
                ),
                new TaskRequirementOptionViewModel(
                    "Patrol",
                    "Roaming gang member with no guarding behavior."
                ),
                new TaskRequirementOptionViewModel(
                    "Patrol StandardScenario",
                    "Roaming gang member that still plays normal scenarios when stopping."
                ),
                new TaskRequirementOptionViewModel(
                    "None StandardScenario",
                    "Static guard that may guard or patrol naturally, with normal scenario idles."
                ),
                new TaskRequirementOptionViewModel(
                    "None AnyScenario",
                    "Ambient NPC that blends into the world with varied scenario idles."
                )
            };
        }
    }
}
