# Handoff: Turret Upgrade Head Sprites — Island Survival TD

## Overview
A sprite pack for upgrade-path visuals on the three turrets. Each turret keeps its existing static base/mount; only the rotating **Head** sprite is swapped when the player commits to an upgrade path and tier. The pack also adds a per-barrel muzzle-flash overlay and per-weapon projectiles.

**Contents**
- 18 head sprites — 3 turrets x 2 paths x 3 tiers, 32x32 transparent PNG
- 36 muzzle-flash overlays — 2 frames per head variant, 32x32 transparent PNG
- 6 projectiles — 8x8 transparent PNG
- `sprite_manifest.json` — machine-readable index of every file, pivot, muzzle offset and projectile mapping
- `reference/` — the original base + head sprites these were derived from (unchanged)
- `Turret Head Variants.dc.html` — visual contact sheet with a live rotation test (open in a browser)

## About the design files
The PNGs in `sprites/` are **final production assets** — import them as-is. The HTML file is a **design reference only**: a contact sheet and rotation rig used to review the art. Do not port the HTML into the game; use it to see what each sprite is meant to look like, how the flash frames sit on each barrel, and how the heads behave when rotated.

## Fidelity
High fidelity. The sprites were built directly on top of the supplied reference head sprites: the mount block, plank seam, red accent, collar and shading ramp are byte-identical to the originals. Only the barrel / funnel-mouth region above the joint is redrawn.

## Art rules the pack follows
- 32x32 canvas, transparent background, hard pixel edges, **point/nearest filtering, no anti-aliasing**.
- **No outlines.** Form is described purely by a 3-4 step value ramp, matching the reference art.
- Light comes from the **left**: a vertical barrel of width n reads `#B6C1CB` (first column) -> `#5C6672` (middle) -> `#3F474F` (last column).
- Muzzle flares are horizontal rows: `#B6C1CB` / `#93A1AD` / `#5C6672` top to bottom.
- Palette is restricted to the reference colors plus the brief's gold and danger red. No new hues.

## Rotation

Every head, flash and projectile is drawn **pointing up** — that is the 0 degree reference. The game rotates the head sprite to aim; the base never rotates.

Set the head sprite's origin to these coordinates, in sprite pixels, measured from the top-left of the 32x32 canvas:

| Turret | Pivot (x, y) |
|---|---|
| `spr_turret` | 16, 20 |
| `spr_turret_long_range` | 16, 11 |
| `spr_turret_mortar` | 16, 19 |

The flash overlay shares the head's canvas and pivot exactly — apply the **same** rotation and origin to both, and draw the flash after (on top of) the head. No separate offset maths needed.

Note: the head sprite is larger than the base's footprint, so at 90 and 270 degrees the barrel extends past the 32x32 base tile. Do not clip the head to the tile.

## Muzzle flash

Two frames per variant, `_f1` (large) and `_f2` (small, fading). Suggested playback: **f1 for ~110 ms, f2 for ~110 ms, then hide** — one fire event. Frame 2 is deliberately dimmer and narrower so the pair reads as a decaying burst.

Multi-barrel variants carry **one burst per barrel** baked into the same overlay, so a spread or double-shot lights up all its muzzles together. The muzzle coordinates below are given in case you want to spawn extra particles, smoke or projectiles from the exact barrel mouths — they are the burst centres in unrotated sprite pixels. Rotate them around the turret's pivot to get world positions.

## Projectiles

8x8, transparent, pointing up. Rotate to the direction of travel.

| File | Used by | Notes |
|---|---|---|
| `spr_turret_bullet.png` | Turret, all tiers except B T3 | Plain iron ball |
| `spr_turret_bullet_fire.png` | Turret B T3 "Vuurschade" | Ember core with a trailing flame |
| `spr_turret_long_range_bullet.png` | Long Range, Path B and A T1 | Pointed slug |
| `spr_turret_long_range_bullet_pierce.png` | Long Range A T2-T3 "Doorborende Kogels" | Hardened dark tip, gold driving band |
| `spr_turret_mortar_shell.png` | Mortar Path A | Fat lit shell |
| `spr_turret_mortar_shell_cluster.png` | Mortar Path B | Smaller fan shell |

## Turret 1 — `spr_turret` (wooden swivel mount, twin-plank body)

Pivot 16, 20. Path A escalates the barrel itself; Path B escalates the mechanism.

| Path | Tier | Upgrade | Head | Flash | Muzzle px | Projectile |
|---|---|---|---|---|---|---|
| A | T1 | Meer Damage | `spr_turret_head_a_t1.png` | `spr_turret_flash_a_t1_f1/f2.png` | (16,3) | `spr_turret_bullet.png` |
| A | T2 | Spreidschot | `spr_turret_head_a_t2.png` | `spr_turret_flash_a_t2_f1/f2.png` | (16,3) (9,7) (22,7) | `spr_turret_bullet.png` |
| A | T3 | Meer Range | `spr_turret_head_a_t3.png` | `spr_turret_flash_a_t3_f1/f2.png` | (16,2) | `spr_turret_bullet.png` |
| B | T1 | Vuursnelheid+ | `spr_turret_head_b_t1.png` | `spr_turret_flash_b_t1_f1/f2.png` | (16,3) | `spr_turret_bullet.png` |
| B | T2 | Dubbelschot | `spr_turret_head_b_t2.png` | `spr_turret_flash_b_t2_f1/f2.png` | (12,3) (19,3) | `spr_turret_bullet.png` |
| B | T3 | Vuurschade | `spr_turret_head_b_t3.png` | `spr_turret_flash_b_t3_f1/f2.png` | (12,3) (19,3) | `spr_turret_bullet_fire.png` |


Visual escalation, Path A: barrel thickened 4px -> 6px with two iron rings (T1); two extra barrels splayed outward from the mount for the spread (T2); full-length reinforced barrel with a side scope and gold lens (T3).
Path B: flywheel cogs with gold hubs on both mount cheeks (T1); two barrels side by side on a shared iron collar (T2); both muzzles ember-hot with smoke wisps rising (T3).

## Turret 2 — `spr_turret_long_range` (tall stone/wood watchtower barrel)

Pivot 16, 11. **Note:** the reference barrel already reaches row 0 of the canvas, so there is no room to make it literally longer. The "Meer Range" tiers escalate through a two-stage barrel, a larger scope and stabilizer fins instead of length.

| Path | Tier | Upgrade | Head | Flash | Muzzle px | Projectile |
|---|---|---|---|---|---|---|
| A | T1 | Meer Range | `spr_turret_long_range_head_a_t1.png` | `spr_turret_long_range_flash_a_t1_f1/f2.png` | (16,2) | `spr_turret_long_range_bullet.png` |
| A | T2 | Doorborende Kogels | `spr_turret_long_range_head_a_t2.png` | `spr_turret_long_range_flash_a_t2_f1/f2.png` | (16,2) (12,3) (20,3) | `spr_turret_long_range_bullet_pierce.png` |
| A | T3 | Meer Range | `spr_turret_long_range_head_a_t3.png` | `spr_turret_long_range_flash_a_t3_f1/f2.png` | (16,2) | `spr_turret_long_range_bullet_pierce.png` |
| B | T1 | Meer Damage | `spr_turret_long_range_head_b_t1.png` | `spr_turret_long_range_flash_b_t1_f1/f2.png` | (16,2) | `spr_turret_long_range_bullet.png` |
| B | T2 | Dubbel Doel | `spr_turret_long_range_head_b_t2.png` | `spr_turret_long_range_flash_b_t2_f1/f2.png` | (11,2) (21,2) | `spr_turret_long_range_bullet.png` |
| B | T3 | Meer Damage | `spr_turret_long_range_head_b_t3.png` | `spr_turret_long_range_flash_b_t3_f1/f2.png` | (10,2) (21,2) | `spr_turret_long_range_bullet.png` |


Path A: ringed barrel plus a compact gold-lens sight (T1); wide notched muzzle brake — a three-pronged piercing head that changes the silhouette, not just the texture (T2); two-stage barrel, larger scope, twin stabilizer fins (T3).
Path B: barrel widened to 5px with two reinforcing rings (T1); Y-split trunk forking into two angled barrels (T2); heavier fork with gold muzzle bands and a gold collar (T3).

## Turret 3 — `spr_turret_mortar` (stone funnel-mouth launcher)

Pivot 16, 19. The mouth is the read: each tier widens or multiplies it.

| Path | Tier | Upgrade | Head | Flash | Muzzle px | Projectile |
|---|---|---|---|---|---|---|
| A | T1 | Meer Range | `spr_turret_mortar_head_a_t1.png` | `spr_turret_mortar_flash_a_t1_f1/f2.png` | (16,10) | `spr_turret_mortar_shell.png` |
| A | T2 | Grotere Splash Radius | `spr_turret_mortar_head_a_t2.png` | `spr_turret_mortar_flash_a_t2_f1/f2.png` | (16,8) | `spr_turret_mortar_shell.png` |
| A | T3 | Meer Range | `spr_turret_mortar_head_a_t3.png` | `spr_turret_mortar_flash_a_t3_f1/f2.png` | (16,7) | `spr_turret_mortar_shell.png` |
| B | T1 | Dubbelschot | `spr_turret_mortar_head_b_t1.png` | `spr_turret_mortar_flash_b_t1_f1/f2.png` | (16,10) | `spr_turret_mortar_shell_cluster.png` |
| B | T2 | Spreidschot | `spr_turret_mortar_head_b_t2.png` | `spr_turret_mortar_flash_b_t2_f1/f2.png` | (16,10) (8,12) (23,12) | `spr_turret_mortar_shell_cluster.png` |
| B | T3 | Meer Damage | `spr_turret_mortar_head_b_t3.png` | `spr_turret_mortar_flash_b_t3_f1/f2.png` | (16,9) (8,11) (24,11) | `spr_turret_mortar_shell_cluster.png` |


Path A: mouth widened two pixels each side (T1); clearly flared mouth on a reinforcing band (T2); widest mouth, banded, with a glowing core in the bore (T3).
Path B: side rack holding two loaded shells (T1); three funnel mouths clustered in a fan (T2); triple cluster with gold reinforcing and glowing shell tips (T3).

## Integration checklist
1. Import all of `sprites/` at point/nearest filtering, no mipmaps, no texture padding that would bleed neighbours.
2. Set each head sprite's origin to its turret's pivot from the table above.
3. On upgrade commit, swap the head sprite to `spr_<turret>_head_<path>_t<tier>.png` and the flash pair to the matching `_flash_` files. `sprite_manifest.json` gives you this mapping directly.
4. On fire, play the flash pair over the head with the same rotation and origin, then hide it.
5. Swap the projectile per the table when the relevant tier is reached.

## Files
- `sprites/` — 60 production PNGs
- `sprite_manifest.json` — index of every variant: pivot, head, flash frames, muzzle offsets, projectile
- `reference/` — the six original base + head sprites, unchanged
- `Turret Head Variants.dc.html` + `support.js` — visual contact sheet and rotation test (design reference, not for the game)
