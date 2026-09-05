# Water tiles — Island Survival TD

Animated seamless water tile set for the gameplay background. Replaces the earlier single
64×64 tile, which read as an obvious grid once it was repeated across a large map.

## Files

Four independently seamless variants, 12 frames each — **48 PNGs, 1024×1024 px**, RGB, fully
opaque (no alpha).

| Variant | Files | Size | Frames |
|---|---|---|---|
| A | `sprites/water_wave_a_01.png` … `_a_12.png` | 1024×1024 | 12 |
| B | `sprites/water_wave_b_01.png` … `_b_12.png` | 1024×1024 | 12 |
| C | `sprites/water_wave_c_01.png` … `_c_12.png` | 1024×1024 | 12 |
| D | `sprites/water_wave_d_01.png` … `_d_12.png` | 1024×1024 | 12 |

Frame order is `01 → 12 → 01`; every loop is closed (frame 12 flows back into frame 01 with the
same delta as any other step). 1024px ÷ 32 PPU = **32 world units** per tile — one tile is wider
than the whole 640×360 reference frame (20×11.25 world units), so at normal zoom there is no
repeat on screen at all.

Total on disk: 27.2 MB for all 48 PNGs.

## Why both bigger *and* four variants

Both were asked for, and they fix different halves of the problem, so the set ships with both:

- **Bigger tile** kills the short-range repeat. At 64px the eye caught the rhythm within two
  tiles; at 1024px a single tile is larger than the entire play view, so there is no rhythm to
  catch at all until the camera pans a full 32 world units.
- **Four variants** kill the long-range repeat. Randomly assigning one of four per grid cell
  means an identical patch recurs at 1-in-4 odds per cell instead of every cell.

## ⚠ VRAM — read this before shipping all four

At 1024×1024 the frame set is large. Uncompressed RGBA32, all frames resident:

| Set | VRAM |
|---|---|
| One variant, 12 frames | **48 MB** |
| Two variants | 96 MB |
| All four variants | **192 MB** |

192 MB of background texture is a lot for modest hardware, and this is the one number in this
handoff worth pushing back on. Three ways out, in the order I would try them:

1. **Ship one or two variants.** At 1024 the tile already exceeds the play view, so variants buy
   much less than they did at 256 — they only matter once the camera pans far. One variant
   (48 MB) is a reasonable default; two (96 MB) covers a large scrolling map.
2. **Drop back to 256×256** — 3.0 MB per variant, 12 MB for all four, and one tile still covers
   most of the view. Re-emit with `W/H: 256`, `ring: 2`; nothing else changes. This is the
   configuration I would pick for anything that has to run on low-end hardware or mobile.
3. **Load frames as a texture array / atlas strip** rather than 12 separate textures if the
   engine side allows it — same bytes, fewer binds, and easier to stream.

The PNGs themselves are 27.2 MB on disk, which also shows up in build size.

## Distribution

Assign a variant **per grid cell**, exactly like the three grass tiles are already scattered
across the field. At 32 world units per tile a "cell" is large, so most maps will only need a handful. Recommended:
hash the cell coordinate rather than calling a random number generator, so the map looks identical
on every load and across save/reload:

```csharp
// 0..3 -> variant A..D, stable per cell
int VariantFor(Vector3Int cell) {
    unchecked {
        int h = cell.x * 73856093 ^ cell.y * 19349663;
        return (h & 0x7fffffff) % 4;
    }
}
```

Per-cluster assignment is pointless at this size — one cell is already 32×32 world units. Per-cell
is the only sensible granularity here.

Even weighting across the four variants is correct. Do not bias toward one variant, and do not
mirror or rotate tiles to fake extra variety — the cell network is non-directional, so a flipped
tile does not read as new, and rotation would break the wrap on non-square tilemaps.

**Seams between different variants:** each tile wraps against *itself*, so where variant A meets
variant B the cell network does not line up. Because the mesh has no direction or flow, this
reads as one more irregularity in the water rather than as a seam — it is the same trade the
grass variants already make. Nothing needs to be done about it.

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
| Max Size | 1024 |

Point filtering, no compression and Repeat wrap are all required — bilinear filtering blurs the
1px cell lines, compression introduces off-palette colors, and Clamp breaks the tiling. Max Size
must be at least 1024 or Unity will downsample and destroy the 1px lines — the default of 2048 is fine, but check it is not lowered by a platform override.

## Tiling

Every variant is seamless in **both** x and y. Verified: zero color discontinuity across the
wrap edges.

Two valid ways to use them:

- **Quad + material**, tiling the UV by `worldSize / 32` units (1024px ÷ 32 PPU = 32 world units
  per tile). Swap the texture per frame.
- **Tiled SpriteRenderer / repeated tiles on a grid.** All tiles must show the *same* frame index
  at the same time, whichever variant they use, otherwise the seams become visible as a
  checkerboard.

Do not offset/scroll the UV to fake motion — the animation is already baked into the frames, and
scrolling on top of it reads as sliding rather than as water.

## Animation

Unchanged from the previous set. Advance one frame every **0.15 s** (≈7 fps, 1.8 s per full loop).
Use a step/discrete change — never blend or cross-fade between frames.

Frame-to-frame difference is at most 5.6% of pixels per variant (A 5.54%, B 5.52%, C 5.51%,
D 5.53%), so the mesh creeps slowly instead of boiling. Going faster than ~10 fps makes it
flicker; slower than ~5 fps makes the crawl read as stuttering.

Drive every water tile on screen from **one** shared timer and one shared frame index, so all
four variants stay in phase.

## Palette

Reuse exactly; no new hues. Identical to the previous tile.

| Role | Hex | Notes |
|---|---|---|
| Base water | `#456DF5` | dominant fill |
| Dark cell | `#3F65EF` | ~10% of cells, quiet depth variation |
| Mesh falloff 1 | `#567DF3` | outermost line step |
| Mesh falloff 2 | `#6A8FEE` | mid line step |
| Mesh core | `#80A2E8` | the cell line itself |
| Sparkle | `#AAC3F1` | occasional brighter line segment |
| Foam speck | `#FFFFFF` | 1–2px, ~5600 per tile (same density per area as before) |

## What the art is doing

Same style rules as the shipped tile, at a larger scale. Base is bright royal blue with a cell
network over it: a voronoi diagram, relaxed, giving round cells of about 8px rather than angular
shards. The lines fade into the water over four steps (`#80A2E8` core → `#6A8FEE` → `#567DF3` →
base), with occasional `#AAC3F1` segments as glints. About 7% of mesh segments are broken out to
base color so the network doesn't read as a rigid grid. Roughly 10% of cells sit a shade darker
for depth. White foam specks blink in and out across different frames.

The cell seeds each orbit a circle of 0.18–0.4px over the 12 frames, which is what makes the mesh
crawl and deform without any pixel travelling far.

**New at this size:** cell spacing is no longer uniform across the canvas. Seeds are placed by
variable-radius blue-noise sampling driven by a low-frequency seamless field, so spacing varies
±30% in soft regions — patches of slightly finer and slightly coarser mesh, a few hundred pixels
across at this canvas size. Relaxation is held to two passes rather than run to convergence, which
rounds the cells off without flattening that regional variation back out. This is what stops a
large tile from looking mechanically regular the way an evenly-spaced one would. Cell size itself
is unchanged at ~8px, so the water looks identical at 1:1 — only the repeat interval grew.

Each tile carries roughly 16,000 cells (A 15,980, B 15,904, C 15,972, D 16,056).

Each variant is a fully independent seed arrangement (own sampling seed), not a crop, shift or
recolor of another.

## Note on adjacent art

Unchanged and still open: the boat sprites (`pirate_boat_*`) carry baked-in foam and wake
highlights authored against the previous darker sea color (`#14496B`). Against `#456DF5` those
highlights read slightly cold. They need a foam recolor pass.

## Regenerating

`water_gen.js` ships with this handoff (plain JS, canvas-based). For small canvases
`buildTile({W, H, seed, rBase, frames, relaxPasses})` returns all frames in one go. At 1024 it is
split in two stages so it fits sane run times:

```js
// stage 1 — seed field (~6s at 1024)
const s = buildSeeds({W: 1024, H: 1024, seed: 20260905, rBase: 6.5,
                      relaxPasses: 2, ring: 1, stride: 2, triesFactor: 0.5});
// stage 2 — frames, in chunks (~3.7s per frame at 1024)
const r = renderFrames(s.seeds, 1024, 1024, 12, makeRng(20260905 ^ 0x5f5f),
                       s.bucketCell, createCanvas, 0, 4, 1);
```

`ring: 1` with the wider bucket cell is a pure speed path — verified pixel-identical to `ring: 2`
on a full 1024² frame (0 differing pixels), 1.5× faster.

The four shipped variants use `rBase: 6.5`, `relaxPasses: 2`, `frames: 12` and seeds
`20260905`, `771043`, `4429117`, `1590883`. Changing only the seed yields another independently
seamless variant in the same style. Changing `W`/`H` re-emits the same style at another size —
use `ring: 2` below 512.
