# Water tiles — Island Survival TD

Animated seamless water tile set for the gameplay background. Hand-off spec for implementation.

## Files

`sprites/water_wave_01.png` … `sprites/water_wave_12.png` — 12 frames, **64×64 px** each, RGB, fully opaque (no alpha).

Frame order is `01 → 12 → 01`; the loop is closed (frame 12 flows back into frame 01 with the same delta as any other step).

## Import settings (Unity)

| Setting | Value |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single |
| Pixels Per Unit | 32 |
| Filter Mode | **Point (no filter)** |
| Compression | **None** |
| Wrap Mode | **Repeat** |
| Generate Mip Maps | off |
| sRGB | on |
| Max Size | 64 |

Point filtering, no compression and Repeat wrap are all required — bilinear filtering blurs the 1px cell lines, compression introduces off-palette colors, and Clamp breaks the tiling.

## Tiling

The tiles are seamless in **both** x and y. Verified: zero color discontinuity across the wrap edges.

Two valid ways to use them:

- **Quad + material**, tiling the UV by `worldSize / 2` units (64px ÷ 32 PPU = 2 world units per tile). Swap the texture per frame.
- **Tiled SpriteRenderer / repeated tiles on a grid.** All tiles must show the *same* frame index at the same time, otherwise the seams become visible as a checkerboard.

Do not offset/scroll the UV to fake motion — the animation is already baked into the frames, and scrolling on top of it reads as sliding rather than as water.

## Animation

Advance one frame every **0.15 s** (≈7 fps, 1.8 s per full loop). Use a step/discrete change — never blend or cross-fade between frames.

Frame-to-frame difference is at most 5.9% of pixels, so the mesh creeps slowly instead of boiling. Going faster than ~10 fps makes it flicker; slower than ~5 fps makes the crawl read as stuttering.

If several water areas are on screen, drive them from **one** shared timer so they stay in phase.

## Palette

Reuse exactly; no new hues.

| Role | Hex | Notes |
|---|---|---|
| Base water | `#456DF5` | dominant fill |
| Dark cell | `#3F65EF` | ~10% of cells, quiet depth variation |
| Mesh falloff 1 | `#567DF3` | outermost line step |
| Mesh falloff 2 | `#6A8FEE` | mid line step |
| Mesh core | `#80A2E8` | the cell line itself |
| Sparkle | `#AAC3F1` | occasional brighter line segment |
| Foam speck | `#FFFFFF` | ~22 specks of 1–2px |

## What the art is doing

Base is bright royal blue with a cell network over it: a voronoi diagram run through 6 Lloyd-relaxation passes, which gives round, evenly spaced cells of about 8px rather than angular shards. The lines fade into the water over four steps (`#80A2E8` core → `#6A8FEE` → `#567DF3` → base), with occasional `#AAC3F1` segments as glints. About 7% of mesh segments are broken out to base color so the network doesn't read as a rigid grid. Roughly 10% of cells sit a shade darker for depth. White foam specks blink in and out across different frames.

The cell seeds each orbit a circle of 0.18–0.4px over the 12 frames, which is what makes the mesh crawl and deform without any pixel travelling far.

## Note on adjacent art

The boat sprites (`pirate_boat_*`) still carry baked-in foam and wake highlights authored against the previous darker sea color (`#14496B`). Against `#456DF5` those highlights read slightly cold. They need a foam recolor pass if the new water ships.
