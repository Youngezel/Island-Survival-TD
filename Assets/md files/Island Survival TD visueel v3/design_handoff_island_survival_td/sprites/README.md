# Sprite-animaties — Island Survival TD

Alle PNG's zijn 1:1 pixelart, geen anti-aliasing, transparante achtergrond waar van toepassing.
Palet en detailniveau zijn overgenomen uit de bestaande sprites in `uploads/`; er zijn geen
nieuwe kleuren geïntroduceerd.

Unity-import voor alles hieronder: Sprite (2D and UI) · Pixels Per Unit **32** ·
Filter Mode **Point (no filter)** · Compression **None** · Generate Mip Maps **uit**.

---

## 1. Torens — base / head / vuur

Elke toren is gesplitst in twee lagen op hetzelfde 32×32 canvas. Leg de `head` als kind-object
bovenop de `base` op exact dezelfde positie; alleen de head roteert.

| Bestand | Inhoud |
|---|---|
| `turret_base.png` | stenen voetstuk, vast |
| `turret_head.png` | koepel + loop, wijst recht omhoog (0°) |
| `turret_head_fire_01..03.png` | vuur-animatie van de head |
| `turret_long_range_base.png` | toren, uitkijkdek, ladder, fundering en vlag — vast |
| `turret_long_range_head.png` | slanke lange loop op compacte stenen kulas, recht omhoog |

Bij de long range turret is de spitse punt uit de base gehaald: die stond in de draaibaan van de
loop. De rode vlag is verplaatst naar een korte mast op de ladderstijl, zodat het silhouet zijn
herkenningspunt houdt en de loop vrij baan heeft. De head rust met zijn kulas op het houten
uitkijkdek.
| `turret_long_range_head_fire_01..03.png` | vuur-animatie |
| `turret_mortar_base.png` | platformslab + breed voetstuk met wielen, vast |
| `turret_mortar_head.png` | trechtermond, recht omhoog |
| `turret_mortar_head_fire_01..03.png` | vuur-animatie |

**Draaipunt** (sprite pivot, zet op *Custom* met deze waarden; zowel base als head krijgen
dezelfde pivot zodat ze exact over elkaar liggen):

| Toren | Pixel (x, y vanaf linksboven) | Genormaliseerde pivot |
|---|---|---|
| Turret | 16, 20 | `0.500, 0.375` |
| Long range turret | 16, 12 | `0.500, 0.625` |
| Mortar | 16, 20 | `0.500, 0.375` |

0° = recht omhoog. Richten: `transform.rotation = Quaternion.Euler(0, 0, -angleToTarget)`
waarbij `angleToTarget = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg`.

**Vuur-animatie**, 3 frames van de head-laag:

| Frame | Inhoud |
|---|---|
| `_fire_01` | rust — identiek aan `_head.png` |
| `_fire_02` | mondingsvuur + loop 2px teruggeduwd langs de loop-as |
| `_fire_03` | 1px teruggeduwd, veert terug naar rust |

Speel als one-shot bij elk schot, ~0.05s per frame (0.15s totaal), daarna terug naar
`_head.png`. Bij de mortar verlaat in frame 2 ook de gouden granaat de mond.

Mondingsvuur gebruikt `#FFCF3F` (kern) en `#F4E4C1` (buitenrand) — beide zitten al in het palet.

---

## 2. Water — `water_wave_01..06.png`

64×64, 6 frames, naadloos herhaalbaar in **beide** richtingen (x en y). Gecontroleerd: 0
kleursprongen op de naden, dus je kunt hem eindeloos tegelen zonder zichtbare randen.

Kleurdekking per frame: `#14496B` 83.5% · `#0D2B40` 14% · `#2286AD` 1.6% · `#4FC9D9` 0.7% ·
`#A9EEF2` 0.2%. De zee leest dus vrijwel volledig als de basiskleur, met brede rustige donkere
dalen en spaarzame korte lichte streepjes op de golfkam — bewust kalm, zodat boten en tegels
de aandacht houden.

De opbouw is één golfband met een periode van 32px (2 banden per tegel) die zacht horizontaal
wiebelt; glinstering zit alleen op de kamlijn en vormt losse streepjes van ~4px, geen doorlopende
lijn en geen losse ruispixels.

Speel op **~8 fps** (0.125s per frame) als loop. De golfbanden lopen diagonaal door de cyclus,
dus je hebt geen extra scroll-offset nodig. Gebruik één quad met tiling material, of een
Tilemap met deze 6 frames als animated tile.

Wil je meer diepteverschil in het spel: leg dezelfde tegel dubbel met de onderste op
`#0D2B40` getint en een offset van 3 frames.

---

## 3. Boten — deining

| Bestand | |
|---|---|
| `pirate_boat_bob_01..04.png` | basis |
| `pirate_boat_fast_bob_01..04.png` | snel |
| `pirate_boat_strong_bob_01..04.png` | zwaar |
| `pirate_boat_distance_bob_01..04.png` | afstand |

4 frames, 32×32, loop. De cyclus combineert een verticale beweging met een lichte kanteling:

| Frame | Verticaal | Kanteling |
|---|---|---|
| 01 | 0 | +1px (rechts omlaag) |
| 02 | −1px | 0 |
| 03 | 0 | −1px (links omlaag) |
| 04 | +1px | 0 |

Silhouet, kleuren en alle details zijn pixel-identiek aan het origineel — alleen de pose
verschuift. Geen enkele pixel valt buiten het canvas.

Speel op **~6 fps** (0.167s per frame, 0.67s per cyclus). Geef elke boot een willekeurige
start-offset (`animator.Play(state, 0, Random.value)`) zodat een golf boten niet in lockstep
deint.

---

## 4. Gras — wind

| Bestand | |
|---|---|
| `tile_grass_1.png` | statische tegel, variant met rots |
| `tile_grass_1_wind_01..04.png` | wind-animatie |
| `tile_grass_2.png` | statische tegel, variant met palmpje |
| `tile_grass_2_wind_01..04.png` | wind-animatie |
| `tile_grass_3.png` | statische tegel, variant met bloemetjes |
| `tile_grass_3_wind_01..04.png` | wind-animatie |

64×56, flat-top hexagon, 4 frames, loop.

**Deze tegels zijn verrijkt.** De originelen in `uploads/` hadden 14–40px detail; deze hebben er
102–136px: grasplukjes, blaadjes, lichte en donkere plukken, bloemetjes, en per variant het
oorspronkelijke kenmerk (rots / palmpje / bloemen) onaangeroerd op zijn plek. Lichte plukken
zitten iets vaker in de bovenhelft en donkere in de onderhelft, wat de tegel volume geeft zonder
aparte shading-laag. Gebruik `tile_grass_N.png` als je de tegel liever stil houdt.

**De hexagon-rand is in elk frame pixel-voor-pixel identiek aan het origineel** — automatisch
gecontroleerd: 0 afwijkende pixels binnen 5px van de rand, in alle 15 bestanden (3 statische +
12 windframes), en 0 pixels buiten het palet. Tegels sluiten dus naadloos aan zonder zichtbare
naden.

Wat beweegt: grasplukjes, blaadjes en bloemetjes wiegen 1px heen en weer, en bij variant 2
alleen de bovenste palmbladeren. Wat vast blijft: rotsjes, kiezels en de palmstam (die wiegen
niet mee in wind — dat las als een fout). Per frame verandert 45–51px, verdeeld over ~30
losse clusters die elk hun eigen fase hebben, dus het veld beweegt niet als één blok.

Elke detailcluster heeft een eigen fase-offset, dus de details bewegen niet in lockstep.

Speel op **~5 fps** (0.2s per frame). Geef naburige tegels een willekeurige start-offset zodat
het veld niet als één blok ademt.

---

## Preview

`sprite_preview.dc.html` in de projectroot laat alle animaties lopen, met de losse frames
eronder en een origineel-naast-verrijkt vergelijking voor de grastegels.

52 PNG's in totaal: 15 toren-lagen, 6 waterframes, 16 bootframes, 15 grastegels.
