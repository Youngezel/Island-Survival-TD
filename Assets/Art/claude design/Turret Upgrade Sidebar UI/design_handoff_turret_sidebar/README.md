# Handoff: Turret Upgrade Sidebar UI — Island Survival TD

## Overview
Pixel-art UI art + layout spec for the turret-upgrade sidebar (`BuildingInspectorUI`) in a 2D
hex-grid tower defense game. Replaces the current flat-color rects with real sliceable textures:
a 9-slice panel frame, four per-state row backgrounds, two buttons, a divider, and ten 16×16 glyphs.

## About the design files
`sprites/` holds the **production art** — 18 PNGs, authored 1:1 against the 640×360 reference frame,
ready to import into Unity as-is. These are the deliverable, not a reference.

`reference/Sidebar Handoff.dc.html` is a **design reference built in HTML** — a mock showing how the
pieces compose and what each state reads like. Do **not** port its markup or CSS. The task is to wire
the sprites into the existing Unity UGUI hierarchy (`BuildingInspectorUI`) using the project's
established prefab/layout patterns, matching the measurements in this README.

## Fidelity
**High-fidelity.** Exact hex values, pixel dimensions, and 9-slice insets are specified and final.
Art is palette-locked: every pixel in every file is one of the 14 hexes below; no hue was invented.
Text is not baked into any texture — all copy stays UGUI `Text`/`TMP`.

## Screen: turret upgrade sidebar

**Purpose** — inspect the selected turret's stats and buy/see upgrade tiers on its two pads,
or sell it.

**Layout** (reference-frame units, 1×):

| | |
|---|---|
| Panel size | 280 × 264 |
| Dock | right edge, vertically centered |
| RectTransform | `anchorMin = anchorMax = (1, 0.5)`, `pivot = (1, 0.5)`, `anchoredPosition = (-8, 0)` |
| Inner padding | 12 on all sides (8px frame + 4px breathing room) → content width 256 |
| Backdrop | gameplay stays visible; dim with a full-screen `#1A1420` at ~55% alpha behind the panel |

Vertical stack, top to bottom:

```
turret name            16px line,  #F4E4C1
3 stat lines           10px each,  label #9DB0C0 / value #F4E4C1, 5px leading
                       DAMAGE: n · RANGE: n TILES · FIRE RATE: n/S
divider (2px)          12px above, 8px below
column headers         PAD A | PAD B, 10px, #F4E4C1, centered per column
6 tier rows            124 × 32 each — 2 columns × 3 rows, 4px row gap, 8px column gutter
SELL button            256 × 32, 8px above
CLOSE button           16 × 16, top-right corner of the content area
```

Rows are authored at 256×32 but sit at **124** wide in the two-column grid — that is what the 6px
insets are for. Say the word if you'd rather have them re-exported native at 124×32.

## Components

### Panel frame — `spr_ui_panel_sidebar.png`, 32×32
`Image` → Sprite, Type **Sliced**, Fill Center **on**, Pixels Per Unit Multiplier **1**.
9-slice border **L 8, B 8, R 8, T 8**. Center is flat `#1A1420`; safe to stretch to any size ≥ 32×32.
Construction, outside in: 1px `#5C3A1E` / 2px `#8B5A2B` / 1px `#5C3A1E` / 1px `#3A2A30` / fill `#1A1420`.
Grain nicks and the four 2×2 gold corner studs (`#C99B1E` with a `#FFCF3F` highlight pixel) live
**inside the corner slices**, so they never smear when the panel stretches.

### Tier row states — 256×32
One sprite per state. Swap the `Image.sprite`; do not tint the row sprites.

| State | Sprite | Type | Insets (L,B,R,T) | Reads as | Interactable |
|---|---|---|---|---|---|
| Locked (needs main-menu unlock) | `spr_ui_row_locked.png` | Sliced | 6,6,6,6 | recessed slot, `#221A20` fill, `#3A2A30` outline, no bevel highlight, 1px `#1A1420` inner shadow top+left | no |
| Purchasable | `spr_ui_row_buy.png` | Sliced | 6,6,6,6 | `#FFCF3F` face, 2px `#C99B1E` bottom shadow + right shadow, `#8B5A2B` outer line | yes |
| Active / owned | `spr_ui_row_active.png` | Sliced | **8**,6,6,6 | `#2A2028` face, full `#FFCF3F` outline, 4px gold tab on the left edge, `#C99B1E` corner notches | no |
| Path-locked (other branch taken) | `spr_ui_row_pathlocked.png` | **Simple** or Tiled | — | `#221A20` under a 50% `#1A1420` checker dither + 8px-pitch `#3A2A30` diagonal hatch | no |

`row_pathlocked` must **not** be stretched — the dither smears. Use it at native 256×32, or set
Image Type = **Tiled** at 124 wide. It is deliberately a different kind of dim from `row_locked`:
locked reads *empty and unbuilt*, path-locked reads *filled but ruled out this run*.

### Buttons
| Sprite | Size | Type | Insets | Detail |
|---|---|---|---|---|
| `spr_ui_btn_sell.png` | 256×32 | Sliced | 6,6,6,6 | `#E0503A` face, 2px `#B83A28` bottom + right shadow, `#5C3A1E` outer line |
| `spr_ui_btn_close.png` | 16×16 | Simple | — | `#E0503A` face, `#B83A28` border, `#F4E4C1` X. Fixed size, never scaled. |

### Divider — `spr_ui_divider.png`, 32×2
Sliced, insets **L 4, R 4, T 0, B 0**. Stretch horizontally only. Row 0 `#3A2A30`, row 1 `#1A1420`.

### Glyphs — all 16×16, transparent, never scaled
State markers, **pre-colored** (do not tint):

| Sprite | Use | Color |
|---|---|---|
| `spr_ui_glyph_padlock.png` | on `row_locked` | `#6F8296` |
| `spr_ui_glyph_check.png` | on `row_active` | `#FFCF3F` |
| `spr_ui_glyph_cross.png` | on `row_pathlocked` | `#B83A28` |
| `spr_ui_glyph_coin.png` | cost prefix on `row_buy` | `#3D2A12` rim / `#C99B1E` / `#FFCF3F` core — designed to read on the gold fill |

Upgrade-type glyphs, **single-tone `#F4E4C1`** — internal detail is carried by empty pixels, never a
second color, so UGUI multiply-tint recolors them to a flat state color without muddying:

`spr_ui_glyph_damage.png` (dagger) · `spr_ui_glyph_range.png` (concentric rings) ·
`spr_ui_glyph_firerate.png` (clock) · `spr_ui_glyph_splash.png` (burst ring) ·
`spr_ui_glyph_pierce.png` (arrow through plate) · `spr_ui_glyph_multishot.png` (three diverging arrows)

Tint via `Image.color`:

| Row state | Glyph tint |
|---|---|
| purchasable | `#3D2A12` |
| active | `#FFCF3F` |
| locked | `#6F8296` |
| path-locked | `#6F8296` |

### Row content layout (inside a 124×32 row)
```
8px  left pad
16   state marker glyph (padlock / check / cross), omitted when purchasable
4    gap
16   upgrade-type glyph
6    gap
—    label, 10px, single line (two lines at 1.4 leading for "(HOOFDMENU)" rows)
auto coin glyph + cost, right-aligned, purchasable rows only
8px  right pad
```

## Text
| Role | Color |
|---|---|
| primary (turret name, stat values, labels) | `#F4E4C1` |
| secondary (stat labels, PAD headers) | `#9DB0C0` |
| disabled (locked + path-locked labels) | `#6F8296` |
| on gold (purchasable labels, cost) | `#3D2A12` |

No outlines, no drop shadows. Snap all text to whole pixels; bitmap font at integer multiples only.

## Interactions & states
- **Locked** and **path-locked** rows: `Button.interactable = false`, no hover, no pressed.
- **Active** rows: not clickable — a status display, not a button.
- **Purchasable** rows: clickable only while `coins >= cost`. When the tier is unlocked and available
  but unaffordable, the brief did not specify a state; current recommendation is `row_buy` with
  `Image.color = #C99B1E` (multiply) and the label at `#3D2A12`, no new asset.
- **Hover / pressed** were not in the state list. When you want them: hover = `row_buy` with the
  2px bottom shadow band, pressed = shadow band collapsed to 0 and content nudged 1px down. Both
  derivable from the existing sprite; ask and I'll cut them.
- Panel show/hide: no animation specified. If you add one, keep it to whole-pixel translation
  (slide in from the right edge) — no scaling or fading of pixel art.

## State the UI needs
Per tier row: `isUnlockedInMeta` (main-menu unlock), `isOwned`, `isPathBlocked` (sibling pad tier
owned), `cost`, `upgradeType` (→ which type glyph). Resolution order for the sprite swap:
`isOwned` → active; else `!isUnlockedInMeta` → locked; else `isPathBlocked` → path-locked;
else purchasable. Panel-level: selected turret ref, name, damage / range / fireRate, sell value.

## Design tokens — the complete palette
No hue outside this list appears in any sprite.

| Hex | Role |
|---|---|
| `#1A1420` | panel background |
| `#221A20` | row background |
| `#2A2028` | slot background |
| `#3A2A30` | divider / inner line |
| `#8B5A2B` | border frame (wood) |
| `#5C3A1E` | outer frame |
| `#FFCF3F` | gold accent — active / affordable |
| `#C99B1E` | gold shadow |
| `#E0503A` | danger red |
| `#B83A28` | danger red shadow |
| `#F4E4C1` | text primary (parchment) |
| `#9DB0C0` | text secondary |
| `#6F8296` | text disabled |
| `#3D2A12` | dark text on gold |

**Spacing scale** (px, 1×): 2 · 4 · 6 · 8 · 12 · 16
**Type scale** (px, 1×): 10 (stats, labels, headers) · 16 (turret name)
**Radius**: none — hard-edged pixel art. Corners are 1px-clipped in the row sprites, baked in.
**Shadows**: none in CSS terms; all depth is baked 1–2px bevel bands inside the sprites.

## Unity import settings — apply to all 18 sprites
| Setting | Value |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single |
| Pixels Per Unit | 32 |
| Filter Mode | **Point (no filter)** |
| Compression | **None** |
| Generate Mip Maps | off |
| Mesh Type | Full Rect |
| Wrap Mode | Clamp |

`CanvasScaler` → Scale With Screen Size, reference resolution **640×360**, Match **0**.
Every sprite is fully opaque or fully transparent — zero semi-transparent pixels — so point
filtering stays crisp at any integer scale. Render the canvas at integer multiples of 640×360.

## Assets
All 18 PNGs in `sprites/` were authored for this handoff. Nothing is third-party, nothing is
licensed from elsewhere, no external font is baked into any texture.

## Files in this bundle
```
README.md                              this document
SPRITE_MANIFEST.md                     condensed manifest: filenames, sizes, insets, import settings
sprites/*.png                          18 production textures
reference/Sidebar Handoff.dc.html      HTML design reference — composition mock, do not port
```

## Open items
1. `row_pathlocked` is Simple/Tiled rather than 9-sliceable because of the dither. Swap the hatch for
   a flat treatment if you need it sliceable.
2. Rows can be re-exported native at 124×32 if you'd rather not slice at all.
3. Hover / pressed / unaffordable variants are derivable from `row_buy` and not yet cut.
4. A mortar turret sprite (e.g. `spr_turret_mortar_head_a_t2.png`) would let the panel frame's wood
   grain match the existing turret art more exactly. The frame is currently plain by design so the
   9-slice edges stay clean under stretch.
