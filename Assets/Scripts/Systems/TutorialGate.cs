using Game.Data;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Lets TutorialController force a specific guided action during an
    /// interactive tutorial step - e.g. "you can only pick up the basic
    /// Turret" or "you can only place it on this one cell" - by having the
    /// normal hotbar/placement code check these null-by-default gates
    /// rather than adding tutorial-specific branches to that code. Both are
    /// null outside a tutorial run, so normal play is never affected.
    /// </summary>
    public static class TutorialGate
    {
        /// <summary>Non-null: only this hotbar item can be picked up (HotbarSlot.OnBeginDrag ignores every other slot).</summary>
        public static HotbarItemData RestrictedHotbarItem;

        /// <summary>Non-null: BuildPlacer.TryPlace only succeeds for this exact cell.</summary>
        public static Vector3Int? RestrictedPlacementCell;
    }
}
