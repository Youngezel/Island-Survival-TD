# Turret Upgrade Sidebar UI — Pixel Art Handoff

Hard-edged pixel art, authored 1:1 against the 640×360 reference frame (32 PPU convention).
No anti-aliasing, no gradients, no colors outside the supplied palette.

## Unity import settings (all sprites)

| Setting | Value |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single |
| Pixels Per Unit | 32 |
| Filter Mode | Point (no filter) |
| Compression | None |
| Generate Mip Maps | off |
| Mesh Type | Full Rect |
| Wrap Mode | Clamp |

Canvas: `CanvasScaler` → Scale With Screen Size, reference resolution 640×360, Match 0.
UGUI `Image` → Sprite, Image Type **Sliced**, `Pixels Per Unit Multiplier` = 1, Fill Center on.

## Sprite manifest

| File | Size | Image Type | 9-slice border (L,B,R,T) | Notes |
|---|---|---|---|---|
| `spr_ui_panel_sidebar.png` | 32×32 | Sliced | 8, 8, 8, 8 | Whole-sidebar frame. Wood double-band + gold corner studs. Center is flat `#1A1420`; safe to stretch to any size ≥ 32×32. Grain nicks live inside the corner slices so they never smear. |
| `spr_ui_row_locked.png` | 256×32 | Sliced | 6, 6, 6, 6 | State 1 — needs main-menu unlock. Recessed, no bevel highlight. |
| `spr_ui_row_buy.png` | 256×32 | Sliced | 6, 6, 6, 6 | State 2 — purchasable. Gold fill, 2px bottom shadow, wood outline. |
| `spr_ui_row_active.png` | 256×32 | Sliced | 8, 6, 6, 6 | State 3 — owned/equipped. Gold outline + 4px gold tab on the left edge (left inset is 8 to protect the tab). Not clickable. |
| `spr_ui_row_pathlocked.png` | 256×32 | **Simple** | — | State 4 — other branch taken. 50% checker dither + diagonal hatch; do **not** stretch (the dither would smear). Use at 256×32, or set Image Type = Tiled. |
| `spr_ui_btn_sell.png` | 256×32 | Sliced | 6, 6, 6, 6 | SELL button. `#E0503A` face, `#B83A28` shadow. |
| `spr_ui_btn_close.png` | 16×16 | Simple | — | CLOSE button, fixed size. |
| `spr_ui_divider.png` | 32×2 | Sliced | 4, 0, 4, 0 | Header/section rule. Stretch horizontally only. |

### State marker glyphs (16×16, transparent, pre-colored)

| File | Use |
|---|---|
| `spr_ui_glyph_padlock.png` | Overlay on `row_locked` (`#6F8296`) |
| `spr_ui_glyph_check.png` | Overlay on `row_active` (`#FFCF3F`) |
| `spr_ui_glyph_cross.png` | Overlay on `row_pathlocked` (`#B83A28`) |
| `spr_ui_glyph_coin.png` | Cost prefix on `row_buy` (gold + `#3D2A12` outline, reads on the gold fill) |

### Upgrade-type glyphs (16×16, transparent, drawn in `#F4E4C1`)

`spr_ui_glyph_damage.png` · `spr_ui_glyph_range.png` · `spr_ui_glyph_firerate.png` ·
`spr_ui_glyph_splash.png` · `spr_ui_glyph_pierce.png` · `spr_ui_glyph_multishot.png`

All six are **single-tone** `#F4E4C1` — internal detail is carried by empty pixels, never a second
color — so `Image.color` multiply-tints them to a flat state color without muddying:

| State | Tint |
|---|---|
| purchasable | `#3D2A12` (dark-on-gold) |
| active | `#FFCF3F` |
| locked | `#6F8296` |
| path-locked | `#6F8296` |

## Layout (reference frame units, 1×)

Panel 280×264, docked right, `anchorMin/Max = (1, 0.5)`, pivot `(1, 0.5)`, `anchoredPosition = (-8, 0)`.
Inner padding 12px on all sides (8px frame + 4px breathing room) → content width 256.

```
header       turret name   16px line
             3 stat lines  10px each
divider      2px            (12px above / 8px below)
column heads PAD A | PAD B  10px
6 tier rows  124×32 each, 4px gap, 8px column gutter
SELL         256×32
```

Two-column rows are 124 wide, so `row_*` sprites are sliced down from 256 — hence the 6px insets.
`row_pathlocked` at 124 wide: set Image Type = Tiled (or re-export at 124 on request).

## Text

Palette-locked, no outlines, no drop shadows except where noted:
primary `#F4E4C1`, secondary `#9DB0C0`, disabled `#6F8296`, on-gold `#3D2A12`.
All text snapped to whole pixels; bitmap font at integer multiples only.

## Not included / open

- No new hues introduced. Nothing sampled outside the supplied list.
- Hover/pressed variants not specified in the brief — say the word and I'll derive them from
  `row_buy` (1px lift on hover, shadow band collapsed on press) without new assets.
- A mortar turret sprite (e.g. `spr_turret_mortar_head_a_t2.png`) would let me match the wood
  grain treatment more exactly on the panel frame. Useful for pass 2, not blocking.
