# Handoff: Island Survival TD — visuele identiteit

## Overzicht
Complete visuele identiteit voor een 2D hex-grid tower defense game in Unity: verdedig een dorpje op een tropisch eiland tegen golven piraten. Dit pakket bevat het kleurenpalet, typografie, sprite-specificaties (4 vijandtypes, dorp, 3 torens, hex-tegels), pixel-exacte mockups van alle vier UI-schermen (HUD, hoofdmenu, wave-keuze, game over) én een map met 53 kant-en-klare geanimeerde PNG-sprites.

## Wat zit erin

| | |
|---|---|
| `README.md` | dit document: alle specs, tokens, schermlayouts |
| `sprites/` | **53 kant-en-klare PNG's** — direct importeerbaar in Unity |
| `sprites/README.md` | pivots, framevolgorde, speelsnelheden, importinstellingen |
| `Island Survival TD - Visuele Identiteit.dc.html` | visuele referentie, open in een browser |
| `support.js` | hoort bij de HTML-referentie, niet nodig voor de implementatie |

## Over de bestanden
De PNG's in `sprites/` zijn **echte assets**: importeer ze en gebruik ze zoals ze zijn.

Het HTML-bestand is een **visuele referentie**, geen productiecode. Het toont de vier
UI-schermen en de art-direction. De sprites daarin zijn met CSS-rechthoeken nagebouwd —
als briefing voor sprites die nog niet bestaan (dorp, boten, tegelvarianten), niet als asset.

## Fidelity
Gemengd — lees dit voordat je begint:

- **Klaar om te gebruiken:** alles in `sprites/` (3 torens gesplitst in base + roteerbare head + vuur-animatie, geanimeerd water, deining voor 4 boten, 3 verrijkte grastegels met wind).
- **Hi-fi spec, nog te bouwen:** de UI-schermen (HUD, hoofdmenu, wave-keuze, game over). Ontworpen op de native cameraresolutie 640×360 en in de HTML 2× vergroot getoond. Alle posities, groottes, kleuren en teksten in dit document zijn in **native pixels** en moeten pixel-exact worden overgenomen.
- **Lo-fi briefing, sprite moet nog getekend:** dorp, de 4 piratenboten en de overige tegelvarianten. Maten, kleurvlakken en silhouet-regels zijn bindend; de precieze pixelplaatsing is aan de artist. Elke boot heeft in de HTML ook een **zwarte silhouet-variant** — dat is de leesbaarheidstest: als het silhouet niet direct van de andere drie te onderscheiden is, is de sprite fout.

---

## Technische basis

| Parameter | Waarde |
|---|---|
| Pixels Per Unit (PPU) | 32 |
| Cameraresolutie (native) | 640 × 360 |
| Hex-tegel | flat-top, 64 × 64 px sprite (= 2 × 2 world units) |
| Units / gebouwen / enemies | 32 × 32 px sprite (= 1 × 1 world unit) |
| Filter Mode | Point (no filter) |
| Compression | None |
| Sprite pivot | units/gebouwen: bottom-center · tegels: center |

**Camera**: orthografisch, `orthographicSize = 360 / (2 × 32) = 5.625`. Gebruik de Pixel Perfect Camera-component met reference resolution 640×360, PPU 32, *Upscale Render Texture* uit, *Pixel Snapping* aan.

**Hex-grid (flat-top)**: Grid → Cell Layout `Hexagon`, Cell Swizzle `YXZ` (dat maakt een pointy-top hex flat-top). Stapafstanden in het ontwerp: horizontaal `0.75 × 64 = 48 px`, verticaal `64 px`, oneven kolommen `+32 px` verticaal verschoven. In world units: x-stap 1.5, y-stap 2.0, offset 1.0.

**Sortering**: `sortingOrder = -Mathf.RoundToInt(worldY * 32)` zodat units op lagere tegels vóór hogere renderen. Sorting layers van achter naar voren: `Water`, `WaterFoam`, `Ground`, `GroundDetail`, `Units`, `Projectiles`, `FX`, `UI`.

**UI-scaling**: Canvas Scaler op `Constant Pixel Size` met `scaleFactor = Mathf.Max(1, Mathf.FloorToInt(Screen.height / 360f))` — alleen hele vermenigvuldigingen, anders wordt pixelart-UI onscherp. Alle UI-rects in dit document zijn in native px; vermenigvuldig niets handmatig.

---

## Design tokens

### Kleuren — water
| Rol | Hex |
|---|---|
| Diep water | `#0D2B40` |
| Open zee | `#14496B` |
| Water midden | `#2286AD` |
| Ondiep water | `#4FC9D9` |
| Schuim | `#A9EEF2` |

### Kleuren — land
| Rol | Hex |
|---|---|
| Gras schaduw | `#2F6B38` |
| Gras basis | `#4F9E42` |
| Gras licht | `#7CC35A` |
| Gras highlight (bovenrand tegel) | `#A3D97F` |
| Zand schaduw | `#C9A063` |
| Zand basis | `#E8C88A` |
| Zand licht | `#DBC9A0` |

### Kleuren — hout & steen
| Rol | Hex |
|---|---|
| Hout donker (outline/onderrand) | `#4A2E17` |
| Hout schaduw | `#5C3A1E` |
| Hout plank | `#6F4520` |
| Hout basis | `#8B5A2B` |
| Hout licht | `#B98246` |
| Steen donker | `#23282E` |
| Steen schaduw | `#3F474F` |
| Steen midden | `#5C6672` |
| Steen basis | `#93A1AD` |
| Steen licht | `#B6C1CB` |

### Kleuren — UI & accenten
| Rol | Hex |
|---|---|
| Goud (primair accent, knoppen) | `#FFCF3F` |
| Goud schaduw (knop-onderrand) | `#C99B1E` |
| Gevaar / health / rood dak | `#E0503A` |
| Rood donker (dakpanlijn) | `#B83A28` |
| Perkament (zeilen, lichte tekst) | `#F4E4C1` |
| Zeil schaduw | `#DBC9A0` |
| Zeil naad | `#C4B18C` |
| UI paneel-achtergrond | `#1A1420` |
| UI rij-achtergrond | `#221A20` |
| UI slot-achtergrond | `#2A2028` |
| UI scheidingslijn | `#3A2A30` |
| UI tekst primair | `#F4E4C1` |
| UI tekst secundair | `#9DB0C0` |
| UI tekst tertiair / disabled | `#6F8296` |
| Paneelrand | `#8B5A2B` |
| Frame buitenrand | `#5C3A1E` |

Regel: **max 2 achtergrondkleuren per scherm**, 3–4 tinten per materiaal (schaduw, basis, licht, accentlijn), **geen gradients**, outline 1px in een donkerder variant van de basis — nooit puur zwart.

### Typografie
| Rol | Font | Grootte (native px) |
|---|---|---|
| Speltitel, schermkoppen | **Pirata One** | 20–46 |
| Alle HUD, knoppen, labels, cijfers | **Silkscreen** | 7, 8, 9, 10, 11, 12, 13, 16, 18 |

Regels: geen anti-aliasing (importeer als bitmap font of gebruik TMP met *Sampling Point Size* = de exacte pt-maat en *Padding* 0); tekst altijd op heel pixel-raster positioneren; geen letterspacing onder 16px. Pirata One uitsluitend boven 20px.

### UI-vormtaal
- **Frame (artboard/paneel)**: 2px rand `#8B5A2B` op vlak `#1A1420`. Grote frames: 4px `#5C3A1E` buiten + 4px `#8B5A2B` binnen.
- **Primaire knop**: vlak `#FFCF3F`, 2px rand `#5C3A1E`, 3px onderschaduw `#C99B1E`, tekst `#3D2A12`.
- **Secundaire knop**: vlak `#2A2028`, 2px rand `#8B5A2B`, tekst `#F4E4C1`.
- **Disabled**: vlak `#2A2028`, rand `#3A2A30`, tekst `#6F8296`, opacity 0.7.
- **Border radius: 0 overal.** Geen ronde hoeken, geen shadows behalve de 3px knop-onderrand.
- **Selectie/hover op een tegel**: 2px binnenrand goud (plaatsbaar) of rood (geblokkeerd) — **nooit** een kleurvulling, anders verdwijnt het gebouw erop.

---

## Sprites — enemies (allemaal 32 × 32, maten in game-pixels)

Alle vier zijn piratenboten die van buiten het scherm naar het eiland varen. Silhouet is het onderscheid; vlagkleur mag tier aanduiden maar mag nooit het enige verschil zijn.

Gedeeld: romp = trapezium (boven breder dan onder), 1px lichte reling bovenop, 1px plankenlijn in het midden, donkere onderrand. Health bar 12 × 2 px direct boven de mast, rood `#E0503A` op `#1A1420`, alleen tonen bij schade. Idle-animatie: 2 frames deining, 4px verticaal, ~0.6s per cyclus.

### 1. Pirate boat (basis)
- Romp 17 × 5, reling `#B98246`, plank `#6F4520`, onderrand `#5C3A1E`
- Roer 2 × 3 aan de achterkant
- Mast 2 × 13 `#5C3A1E`, ra (horizontale balk) 12 × 1 `#8B5A2B`
- Vierkant zeil 9 × 9 `#F4E4C1`, horizontale streep 9 × 2 `#E0503A`, rechterrand 2px `#DBC9A0`
- Vlag 5 × 2 `#E0503A` op paal, bemanningsstip 2 × 2 `#E8C88A` met rode bandana 2 × 1
- Vat 3 × 2 `#B98246` op het dek
- Boegschuim: 7 × 1 en 11 × 1 `#A9EEF2`

### 2. Fast pirate boat
- Romp 26 × 3 (lang en plat), reling `#DBC9A0`, onderrand `#8B5A2B`
- Boegspriet 4 × 1 `#5C3A1E` vooruit, riem 5 × 1 achter
- Mast 2 × 18 (hoogste dunne mast), schuin driehoekszeil 10 × 13 `#F4E4C1` met schaduwhelft `#DBC9A0`
- Kleiner voorzeil (fok) 6 × 8, driehoek gespiegeld
- Vlag 4 × 2 `#E0503A`
- Zwaar boegschuim: 12 × 2 `#A9EEF2` + 8 × 1 `#4FC9D9` — leest als snelheid

### 3. Strong pirate boat
- Romp 27 × 8 (breed en blokkig), 2 plankenlijnen `#4A2E17`, reling `#8B5A2B`
- 4 stalen platen 4 × 4 `#93A1AD` met 1px onderrand `#5C6672`, gelijkmatig over de romp
- Mast 3 × 13, kraaiennest 7 × 3 `#8B5A2B` met donkere onderrand
- Groot dubbel zeil 16 × 10 `#DBC9A0`, 2 verticale naden `#C4B18C`, schedelmarkering 3 × 3 `#F4E4C1` met 2 donkere oogpixels
- Zwarte vlag 6 × 2 `#1A1420`
- Breed schuim 30 × 2 `#A9EEF2` + 24 × 1 `#4FC9D9`

### 4. Distance pirate boat
- Romp 19 × 5
- **Kanon**: affuit 6 × 6 `#93A1AD` met onderrand `#5C6672`, loop 10 × 3 `#5C6672` die **buiten de romp uitsteekt**, mondring 2 × 5 `#93A1AD`, 2 loopbanden 1 × 3 `#3F474F`
- Mast 2 × 11 achteraan, klein zeil 6 × 7 met rode streep
- Kruitvat 3 × 3 `#B98246`, bemanningsstip 2 × 2
- Zwarte vlag 5 × 2
- Silhouet-eis: harde horizontale punt vooraan (de loop) — dat is het herkenpunt

---

## Sprites — gebouwen

### Dorp (het doel)
32 × 32, staat altijd op de middelste tegel. **Enige object in het spel met een rood dak + gouden vlag** — die combinatie mag nergens anders voorkomen. Ongeveer 1,5× de hoogte van een toren.

- Romp 20 × 10 `#B98246`, bovenrand 1px `#DBC9A0`, plankenlijn `#8B5A2B`, sokkel 20 × 4 `#8B5A2B` met onderrand `#5C3A1E`
- Dak driehoek 24 × 8 `#E0503A`, 2 dakpanrijen 1px `#B83A28`, dakrand 25 × 1 `#8B5A2B` met overstek
- Deur 4 × 6 `#5C3A1E` met donkere kop en gouden klink 1 × 1
- 2 ramen 4 × 3 `#4FC9D9` met kruisverdeling `#5C3A1E`
- Schoorsteen 3 × 6 `#93A1AD` met kop `#5C6672`; rookpluimen 2 × 2 en 3 × 2 `#DBE3EA` op opacity 0.7 / 0.45 (2 frames, langzaam opstijgend)
- Vlaggenmast 2 × 9 `#5C3A1E`, gouden wimpel 6 × 4 `#FFCF3F` met inkeping rechts, dwarsbalk 4 × 1
- Op de tegel eromheen: 2 kisten 4 × 2 `#B98246`, padstenen

**Schadestaat**: dak verkleurt naar `#8B3A2B`, vlag hangt half. Health van het dorp staat in de HUD, niet op de tile.

### Turret (standaard)
32 × 32 op een hex-tegel. Compact en laag — het referentiepunt voor de andere twee.
- Stenen voetstuk 24 × 14 `#93A1AD`, bovenrand 2px `#B6C1CB`, voeg 1px `#5C6672`, 2 verticale voegen, sokkel 24 × 4 `#5C6672`
- Houten geschutkoepel 16 × 10 `#8B5A2B` met lichte bovenrand en donkere onderrand
- Loop 14 × 4 `#5C6672` met lichte bovenlijn + mondring 3 × 6 `#93A1AD`
- Detail: zandzak/vat 7 × 6 `#B98246` links, kist 6 × 5 rechts, luik 4 × 4 `#3F474F`, rode markering 4 × 4 `#E0503A`

### Long range turret
Smalste silhouet, steekt boven de tegel uit.
- Schacht 12 × 26 `#93A1AD`, linkerkant 3px `#B6C1CB`, rechterkant 3px `#5C6672`, 2 horizontale voegen
- Kijkgat 4 × 5 `#1A1420`
- Uitkijkplatform 20 × 8 `#8B5A2B` met lichte bovenrand en donkere onderrand
- Spits dak 8 × 8 `#F4E4C1` (driehoek) met schaduwhelft `#DBC9A0`, vlaggetje 5 × 2 `#E0503A` erboven
- Ladder: staander 2 × 18 `#8B5A2B` + 2 sporten 6 × 1
- Voet 24 × 6 `#5C6672`, vat 6 × 5 ernaast
- Loop 10 × 3 `#5C6672` horizontaal

### Mortar
Laag, breed, zwaar. Silhouet is een trapezium — géén loop.
- Basis 26 × 12 `#5C6672`, bovenrand 2px `#93A1AD`, voeg 1px `#3F474F`, 2 verticale voegen, sokkel 26 × 4 `#3F474F`
- Trechtermond 16 × 10 `#93A1AD`, trapezium (breed onder, smaller boven), linkerhelft `#B6C1CB`, horizontale naad `#5C6672`
- Kamer 8 × 6 `#3F474F` met opening `#23282E` bovenop
- Gouden granaat 3 × 3 `#FFCF3F` in de lucht boven de mond (+ 2 kleinere op lagere opacity als vuur-hint)
- Wielen/kogels: 3 rondjes 3 × 3 `#3F474F` / `#23282E`, houten steunblokken 3 × 4 `#8B5A2B` links en rechts

---

## Sprites — omgeving

### Hex-grondtegel (64 × 64, flat-top)
Clip-vorm: `polygon(25% 0, 75% 0, 100% 50%, 75% 100%, 25% 100%, 0 50%)` — dus flat top/bottom, punten links en rechts.

Opbouw van boven naar onder (geeft volume zonder aparte shading-sprite):
1. Grasvlak `#4F9E42` over de hele hex
2. Bovenrand strook 32 × 3 `#A3D97F`
3. Onderste zone `#2F6B38` (gras schaduw)
4. Strandband onderaan `#C9A063`, daarop lichter zand `#E8C88A`
5. Schuimlijn onderaan 3px `#A9EEF2` langs de onderrand

Detail (verspreid, 2–4 px blokjes): grasplukken `#7CC35A` en `#2F6B38`, blad-highlights `#A3D97F`, 1 gele bloem 2 × 2 `#FFCF3F`, 1 rode bloem 2 × 2 `#E0503A`, rots 6 × 4 `#93A1AD` met onderrand `#5C6672`, palmboom (stam 2 × 8 `#8B5A2B` + 3 bladeren 5–8 × 2 in `#2F6B38` / `#4F9E42`), 1 schelp 3 × 2 `#F4E4C1` op het zand.

### Tegelvarianten
| Variant | Basis | Detail |
|---|---|---|
| Gras | `#4F9E42` | grasplukken, bloem, lichte bovenrand |
| Zand | `#E8C88A` | schaduwzone `#C9A063`, schelpen, korrels |
| Jungle | `#2F6B38` | palmboom, dichte bladeren `#4F9E42` / `#7CC35A` |
| Plaatsbaar (koopbaar) | `#4FC9D9` (ondiep water) | 2px binnenrand `#FFCF3F` |
| Geblokkeerd | `#14496B` | 2px binnenrand `#E0503A` |

### Water
- Basis: verticale streepbanden `#14496B` / `#175074`, band-breedte 12px (subtiel, geen ruis)
- Ondiepe ring rond het eiland: `#4FC9D9` op ~30% opacity
- Golflijnen: losse streepjes 10–14 × 1 px in `#4FC9D9`, verspreid, langzaam horizontaal scrollend (≈4 px/s)
- Schuimkoppen: streepjes 8–12 × 1 px `#A9EEF2`, minder frequent, langs de eilandrand dichter op elkaar
- Nooit meer dan ~10 golflijnen per 640×360 frame zichtbaar

### Eilandopbouw
Het eiland groeit naar buiten vanuit de dorpstegel. Vrije randtegels worden getoond als ondiep water met gouden hex-omtrek, zodat de groei-richting altijd leesbaar is. Startopstelling in het ontwerp: 7 landtegels (1 dorp-centrum + ring) en 2 koopbare randtegels. Spawn van piraten: buiten het beeld, in het open water rondom; ze varen naar de dichtstbijzijnde landrand.

---

## Scherm 1 — HUD (in-game)

Canvas 640 × 360. Balken samen 22% van de hoogte; de rest blijft speelveld.

### Bovenbalk
- Rect: x 0, y 0, 640 × 28, vlak `#1A1420`, onderrand 2px `#8B5A2B`, padding 8px links/rechts
- **Links — dorp-health**: hart-icoon 10 × 10 `#E0503A`; balk 92 × 10, vlak `#3D2028`, 1px rand `#8B5A2B`, 1px padding, vulling `#E0503A`; tekst `72/100`, Silkscreen 8px `#F4E4C1`
- **Midden — munten**: rond icoon 10 × 10 `#FFCF3F` met 2px binnenrand `#C99B1E`; bedrag Silkscreen 10px `#FFCF3F` (`340`)
- **Rechts — wave**: label `WAVE` 8px `#9DB0C0`; nummer 12px `#F4E4C1` (`12`); timer `· 0:24` 8px `#6F8296`
- Alle drie groepen: horizontale flow met 6px gap

### Hotbar (onder)
- Rect: x 0, y 308, 640 × 52, vlak `#1A1420`, bovenrand 2px `#8B5A2B`, inhoud gecentreerd, 6px gap
- 5 slots 40 × 40, vlak `#2A2028`, 2px rand `#5C3A1E`; **geselecteerd slot: rand `#FFCF3F`**; disabled slot: vlak `#221A20`, rand `#3A2A30`, opacity 0.7 + diagonale rode streep
- In elk slot: mini-sprite van het gebouw (zelfde opbouw als de wereld-sprite), hotkey-cijfer 7px `#6F8296` linksboven, kosten 7px `#FFCF3F` rechtsonder (grijs als onbetaalbaar)
- Slot-inhoud in volgorde: Turret `50`, Long range turret `90`, Mortar `120`, Hex-tegel `200`, (locked) `300`
- Daarna 2px verticale scheiding `#3A2A30` en een instellingen-slot 40 × 40 met tandwiel

### Tooltip (bij selectie)
- Rect: rechts uitgelijnd, 8px van de rand, 60px boven de onderkant; vlak `#1A1420`, 2px rand `#8B5A2B`, padding 5 × 7
- Regels: naam 8px `#FFCF3F` (`TURRET`), stats 7px `#9DB0C0` (`dmg 12 · rng 3`), hint 7px `#6F8296` (`klik tegel om te plaatsen`)

---

## Scherm 2 — Hoofdmenu + upgrade-winkel

Canvas 640 × 360. Achtergrond: zee met golflijnen, bovenste 120px lichter (`#2286AD` op 25%), links onderin een eilandsilhouet.

- **Titel**: `Island Survival TD`, Pirata One 46px `#FFCF3F`, gecentreerd, y 26, tekstschaduw 3px offset `#5C3A1E`
- **Subtitel**: `VERDEDIG HET DORP TEGEN DE PIRATEN`, Silkscreen 8px `#A9EEF2`, letterspacing 3px, y 76
- **Knoppenkolom** links: x 32, y 116, breedte 200, 10px gap
  - `SPEEL` — 200 × 34, primaire knopstijl, Silkscreen 16px
  - `INSTELLINGEN` — 200 × 22, secundair, 9px
  - `STOPPEN` — 200 × 22, secundair, 9px
  - Daaronder (8px extra): paneel met `BESTE RUN` 8px `#6F8296` en `WAVE 18` 12px `#F4E4C1`
- **Upgrade-winkel** rechts: x 268 (rechts 24px marge), y 108, breedte 348, paneelstijl, padding 10
  - Kop: `Upgrade-winkel` Pirata One 20px `#FFCF3F`; rechts `XP` 8px `#9DB0C0` + `2.450` 13px `#4FC9D9`; 2px scheidingslijn `#3A2A30` eronder
  - 3 rijen (Turret / Long range turret / Mortar), elk 8px gap: vlak `#221A20`, 1px rand `#3A2A30`, padding 6
    - Icoon 28 × 28 in slotstijl met mini-sprite
    - Naam Silkscreen 9px `#F4E4C1`; daaronder `LVL n` 7px `#6F8296` + 5 level-pips 8 × 4 (gevuld `#FFCF3F`, leeg `#3A2A30`)
    - Rechts: knop 20px hoog, padding 0 8, primaire stijl, tekst 8px `UPGRADE` + kosten in `#5C3A1E` (`600 XP` / `250 XP`); op max level: vlak `#2A2028`, rand `#3A2A30`, tekst `MAX` `#6F8296`
  - Voetregel 7px `#6F8296`: `XP verdien je per wave — upgrades blijven tussen runs behouden.`

Waarden in het ontwerp: Turret lvl 3 (600 XP), Long range turret lvl 1 (250 XP), Mortar lvl 5 (MAX).

---

## Scherm 3 — Wave-keuzescherm

Verschijnt tussen twee waves, boven het bevroren speelveld: overlay `#0D1520` op 72% opacity.

- **Bovenregel**: `WAVE 12 OVERLEEFD`, Silkscreen 9px `#A9EEF2`, letterspacing 3px, y 36, gecentreerd
- **Kop**: `Kies je buit`, Pirata One 30px `#FFCF3F`, y 54
- **Twee kaarten** 200 × 150, paneelstijl, y 118, x 96 en x 344, padding 12, inhoud gecentreerd, 8px gap
  - Links: gouden munt-icoon 34 × 34 (rond, 5px binnenrand `#C99B1E`) → `+150 MUNT` 12px `#FFCF3F` → uitleg 8px `#9DB0C0`: `Direct te besteden aan torens deze run` → knop `KIES` 100% × 22, primaire stijl, 10px
  - Rechts: hex-icoon 44 × 34 `#4F9E42` → `GRATIS HEX-TEGEL` 12px `#7CC35A` → uitleg 8px `#9DB0C0`: `Eiland groeit met 1 tegel — permanent bouwveld` → knop `KIES` secundaire stijl (hover/focus wisselt naar primair)
- **Timer**: regel 8px `#6F8296` op y 290: `WAVE 13 START OVER 0:08 — kiezen is verplicht`; balk 208 × 6 op x 216, y 310, vlak `#2A2028`, 1px rand `#5C3A1E`, vulling `#E0503A` (loopt leeg). Bij 0:00 automatisch de linkerkaart kiezen.

---

## Scherm 4 — Game over

Overlay `#2A1018` op 78% opacity boven het speelveld.

- **Kop**: `Het dorp is gevallen`, Pirata One 40px `#E0503A`, y 44, gecentreerd, tekstschaduw 3px `#3D1418`
- **Resultatenpaneel**: x 168, y 116, breedte 304, paneelstijl, padding 14, 10px gap. Drie rijen label links / waarde rechts, elk met 1px scheidingslijn `#3A2A30`:
  - `WAVE BEREIKT` 9px `#9DB0C0` — `12` 18px `#F4E4C1`
  - `PIRATEN GEZONKEN` 9px — `184` 13px `#F4E4C1`
  - `XP VERDIEND` 9px — `+620` 18px `#4FC9D9`
  - Voetregel 7px `#6F8296`: `Beste run: wave 18`
- **Knoppen**: x 168, y 276, 304 breed, 8px gap, elk 28px hoog, 11px tekst
  - `HOOFDMENU` — primaire stijl
  - `NOG EEN RONDE` — secundaire stijl

---

## Interacties & gedrag

- **Tegel selecteren**: hover toont 2px gouden binnenrand; klikken met een gekozen gebouw plaatst het als er munten genoeg zijn, anders knipperen de munten in de HUD 2× rood (~120ms per frame).
- **Hotbar**: hotkeys 1–5; geselecteerd slot krijgt gouden rand; tooltip verschijnt rechtsonder.
- **Onbetaalbaar**: kostenlabel in het slot wordt `#6F8296`, slot blijft klikbaar maar plaatsen faalt.
- **Wave-flow**: wave loopt → alle boten verslagen → wave-keuzescherm (8s timer, kiezen verplicht) → volgende wave.
- **Dorp-health**: raakt een boot de landrand, dan schade; balk krimpt met een korte tween (~150ms). Bij 0 → game over.
- **Game over**: overlay fade-in ~250ms, paneel schuift 8px omhoog in dezelfde tijd.
- **Animaties in-world**: gebruik de kant-en-klare frames uit `sprites/` — boot-deining 4 frames op ~6 fps, water 6 frames op ~8 fps, gras 4 frames op ~5 fps, toren-vuur 3 frames op ~20 fps als one-shot. Schoorsteenrook op het dorp: 2 frames / 1.2s. Alles op hele pixels — nooit sub-pixel bewegen. Geef elke boot en elke grastegel een willekeurige start-offset zodat ze niet in lockstep bewegen.
- **Transities tussen schermen**: hard cut of maximaal een 150ms fade. Geen easing-curves die pixels laten smeren.

## State

| State | Scope | Beschrijving |
|---|---|---|
| `villageHealth` / `villageHealthMax` | run | HUD bovenbalk |
| `coins` | run | verdiend per verslagen boot, besteed in hotbar |
| `waveNumber`, `waveTimer` | run | HUD + wave-keuze |
| `tiles[]` | run | positie, type, bezetting per hex |
| `placedBuildings[]` | run | type, level, tegel |
| `xp` | persistent | upgrade-winkel |
| `upgradeLevels{turret, longRange, mortar}` | persistent | 1–5 per type |
| `bestWave` | persistent | hoofdmenu + game over |

Persistente waarden: `PlayerPrefs` of een JSON-save; run-state leeft in een `GameState`-object.

## Sprite-animaties (`sprites/`)
53 kant-en-klare PNG's, afgeleid van de definitieve sprites: toren-lagen (base + roteerbare
head + 3-frame vuur-animatie per toren), een naadloos tegelbare geanimeerde watertegel (64×64,
6 frames), deining voor de 4 piratenboten (4 frames elk) en verrijkte grastegels met
wind-animatie (3 varianten, statisch + 4 frames elk, hexagon-rand pixel-identiek aan het
origineel). Zie `sprites/README.md` voor pivots, framevolgorde, speelsnelheden en
Unity-importinstellingen.

## Assets
Geen externe assets nodig. Alle sprites moeten als pixelart .png's gemaakt worden volgens de specs hierboven (32 × 32 voor units/gebouwen, 64 × 64 voor tegels). Fonts: **Pirata One** en **Silkscreen**, beide via Google Fonts (SIL Open Font License) — importeer als TextMeshPro-asset met Point-sampling. In de HTML-referentie staan op het stijlgids-artboard twee lege slots voor echte pixelart-referentiebeelden; die zijn nog niet gevuld.

## Bestanden
- `Island Survival TD - Visuele Identiteit.dc.html` — het volledige ontwerp: 8 artboards (1a stijlgids, 1b enemies, 1c gebouwen, 1d omgeving, 1e HUD, 1f hoofdmenu, 1g wave-keuze, 1h game over). Open in een browser; de mockups van 1e–1h staan 2× vergroot maar zijn op 640 × 360 ontworpen.
- `support.js` — runtime die bij het HTML-bestand hoort (alleen nodig om de referentie te bekijken, niet om te implementeren).
