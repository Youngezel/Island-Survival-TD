# CLAUDE.md — Island Survival TD

Projectcontext voor Claude Code. Lees dit voordat je code schrijft. Afgeleid van de GDD (v0.1).

---

## 1. Wat we bouwen

**Island Survival TD** — een 2D top-down **tower defence** in Unity, solo ontwikkeld.

**Pitch:** Verdedig een dorpje op een eiland tegen golven piraten. De map bestaat uit flat-top hexagon-tiles; per wave kun je nieuwe tiles en verdedigingsgebouwen bijplaatsen om te blijven overleven.

- Genre: 2D top-down tower defence
- Doelplatform: PC (Steam)
- Besturing: muis (camera pannen door slepen; gebouwen slepen uit hotbar naar hex-tile)
- Setting: eiland omringd door water; piraten komen met schepen aan en vallen het dorp aan
- Structuur: oneindig, steeds hogere waves. Geen win-conditie; verlies wanneer het dorpje kapot is. Na verlies krijg je XP om gebouwen te upgraden in het menu.

---

## 2. Belangrijk: wat jij wel en niet doet

- **Wel:** C#-scripts, ScriptableObjects, editor-tooling, mappenstructuur, refactors, bugfixes.
- **Wel (via Unity MCP server):** ik gebruik in Unity een Unity MCP server, dus je hebt directe toegang tot de Unity Editor via MCP-tools. Je kunt zelf scenes opbouwen, GameObjects en prefabs aanmaken/koppelen, componenten toevoegen en configureren, Inspector-velden invullen, ScriptableObject-assets aanmaken, en de hex-tilemap tekenen/aanpassen. Gebruik hiervoor de beschikbare Unity MCP-tools in plaats van te wachten tot ik het handmatig doe.
- **Niet:** dingen die buiten de Unity Editor vallen en niet via de MCP-tools bereikbaar zijn (bv. externe art/audio zelf tekenen of inspreken, bestanden van buiten het project importeren zonder dat ik ze aanlever, Steam-pagina/publishing-zaken).
- **Daarom:** alleen wanneer iets écht niet via MCP lukt, sluit je dat onderdeel af met een blokje **"Editor-stappen"** — wat ik dan alsnog handmatig in Unity moet doen. Dit is de uitzondering, niet de standaard.

---

## 3. Tech stack (pinned — niet wijzigen zonder overleg)

| Onderdeel | Keuze |
|---|---|
| Unity-versie | Unity 6.3 LTS — **6000.3.9f1** |
| Render pipeline | URP 2D |
| Input | Nieuw Input System package |
| Camera | Cinemachine + Pixel Perfect Camera |
| Tiles | Unity Hexagonal Tilemap (flat-top) voor de grond; gebouwen/enemies/dorpje zijn losse prefabs erbovenop |
| Save | JSON in `Application.persistentDataPath` |
| Version control | Git + Unity `.gitignore` |
| Packages | 2D URP, Input System, 2D Tilemap Editor (Hexagonal), Cinemachine |

**Art:** pixel-art. PPU **32**. Hex-tiles **64×64 px** (flat-top). Units/turrets/enemies **32×32 px**. Camera-resolutie **640×360** (Pixel Perfect; verhoogd vanaf 320×180 na overleg — 320×180 voelde te ingezoomd voor tower-defense overzicht).
**Sprite-naamgeving:** `spr_` prefix (bv. `spr_turret_long_01`, `spr_enemy_fast`, `spr_tile_grass`).

### Kleurenpalet (16 kleuren — "Island Survival")

Vast palet; gebruik alleen deze kleuren voor alle sprites en UI.

| Rol | Hex |
|---|---|
| Diep water | `#173a5e` |
| Water | `#2d6e93` |
| Ondiep water / schuim | `#6fc6d1` |
| Zand licht | `#f4e4b0` |
| Zand donker | `#d6ad6b` |
| Gras licht | `#82c34b` |
| Gras donker | `#4a8b3a` |
| Loof donker | `#2e5a30` |
| Hout licht | `#b5793e` |
| Hout donker | `#6e4326` |
| Steen licht | `#9aa4ad` |
| Steen / metaal donker | `#55606b` |
| Piraat rood (gevaar) | `#c8402f` |
| Goud / munten | `#f2c14e` |
| UI licht (bijna wit) | `#f7f3e6` |
| Outline (bijna zwart) | `#1e1a24` |

> Dit is een zelfgekozen startpalet. Wil je later switchen naar een Lospec-palet, vervang dan deze tabel — de rest van de spec verandert niet.

---

## 4. Projectstructuur (Assets/)

```
Assets/
  Art/               # sprites, hex-tiles
  Audio/
  Prefabs/           # dorpje, gebouwen, enemies, projectielen
  Scenes/            # Bootstrap, MainMenu, Game
  ScriptableObjects/ # data-assets (enemies, buildings, waves)
  Scripts/
    Grid/            # hex-grid, tile-plaatsing
    Buildings/       # gebouwen/turrets + targeting/schieten
    Enemies/         # enemy-gedrag + pathing
    Combat/          # projectielen, damage, splash
    Waves/           # wave-spawning + tussen-wave keuze
    Economy/         # munten (in-run) + XP (meta)
    Systems/         # game state, save/load
    UI/              # HUD, hotbar, menus
    Data/            # ScriptableObject class-definities
  Settings/          # URP, Input Actions, Pixel Perfect
```

---

## 5. Coding conventions

- Eén publieke class per bestand; bestandsnaam == classnaam.
- Namespace per feature: `Game.Grid`, `Game.Buildings`, `Game.Enemies`, enz.
- `PascalCase` types/methods, `camelCase` locals, `_camelCase` private fields.
- Geen magic numbers — waarden in `[SerializeField]` of ScriptableObject-data.
- **Alle stats in ScriptableObjects**, niet hardcoded in gedrag-scripts.
- Ontkoppel met C#-events (bv. `OnVillageDestroyed`, `OnWaveCleared`, `OnCoinsChanged`).
- Geen `Find`/`GetComponent` in `Update` — cache in `Awake`.

---

## 6. Architectuur-principes

- **Data-driven:** enemies, gebouwen en waves zijn ScriptableObject-assets. Nieuwe enemy/gebouw = nieuw asset, geen nieuwe class (tenzij ander gedrag zoals splash).
- **Gedeeld gedrag als losse components:** `Health`, `Targeting`, `Shooter`, `Projectile` worden hergebruikt door dorpje én turrets.
- **Hex als basis:** één hex-grid systeem dat wereldcoördinaten ↔ hex-coördinaten omrekent; enemies en plaatsing werken hier bovenop.
- **Scenes:** Bootstrap → MainMenu (met upgrades) → Game.

---

## 7. Systemen (bouw in deze volgorde, één per keer)

Bouw incrementeel. Wacht na elk systeem op mijn test-feedback. Geef per systeem: scripts, korte uitleg, **Editor-stappen**.

### 7.1 Hex-grid
- Flat-top hex-tilemap. Conversie wereld ↔ hex-cel. Bepaal buur-tiles.
- Tiles hebben een bezet/vrij-status (voor plaatsing).
- Acceptatie: ik kan tiles in de editor tekenen; code herkent welke cel onder de muis ligt.

### 7.2 Camera
- Cinemachine + Pixel Perfect. Pannen door **linkermuisknop ingedrukt te houden en te slepen**. Begrens binnen de map.
- Acceptatie: soepel pannen, stopt aan de randen.

### 7.3 Dorpje (core)
- Vast startpunt van elke run — **niet koopbaar**, staat er vanaf het begin.
- `Health` + `OnVillageDestroyed`. Het dorpje schiet ook (range/damage uit §11).
- Verlies = dorpje op 0 HP → game over.
- Acceptatie: dorpje neemt schade, game-over triggert.

### 7.4 Enemy base + data
- `EnemyData` (SO): naam, maxHP, damage, coinReward, moveSpeed, range, attackRate (schoten/sec).
- `Enemy`: spawnt op spawnpunt, beweegt naar de dichtstbijzijnde hex-tile/dorp, valt aan binnen range met `attackRate`. Ranged enemies (range > 1) schieten van afstand.
- Bij dood: geef `coinReward`.
- Acceptatie: één piraat vaart naar het dorp, doet schade, geeft munten bij dood.

### 7.5 Buildings / turrets + data
- `BuildingData` (SO): naam, maxHP, damage, cost, range, fireRate, splash (bool).
- `Building`: staat op een hex-tile, target de dichtstbijzijnde enemy binnen range, schiet projectielen (mortar = splash).
- Acceptatie: een turret op een tile schiet automatisch op enemies in range.

### 7.6 Projectielen & damage
- `Projectile` (schade, snelheid), `Health.TakeDamage`, splash-radius voor de mortar.
- Acceptatie: kogels raken, HP daalt, splash raakt meerdere enemies.

### 7.7 Plaatsing (bouwen)
- Sleep een gebouw uit de **hotbar-UI** naar een vrije hex-tile; munten worden afgetrokken.
- Tile zelf is ook koopbaar/plaatsbaar (zie §11).
- Acceptatie: ik koop en plaats een turret; munten kloppen; bezette tile weigert een tweede.

### 7.8 Wave-systeem
- Waves spawnen enemies; per wave moeilijker (meer/sterkere enemies).
- Tutorial-level = 5 rondes; main-level = oneindig.
- **Tussen waves:** speler kiest **extra munten óf een extra hex-tile** om te plaatsen.
- Acceptatie: waves lopen op, keuze-scherm verschijnt na elke wave.

### 7.9 Economy
- Munten (tijdens een run, voor bouwen). XP (meta, na verlies → gebouw-upgrades in menu).
- Acceptatie: munten stijgen bij kills; XP wordt toegekend bij game over.

### 7.10 UI
- HUD: dorp-health, munten, wave-nummer. Hotbar met koopbare gebouwen. Tussen-wave keuze. Hoofdmenu (met upgrades), pauze, game over.
- Losgekoppeld via events uit §7.3/7.9.
- Acceptatie: HUD update live; hotbar en keuze-scherm werken.

### 7.11 Save / meta-progressie
- JSON in `persistentDataPath`: XP en vrijgespeelde/geüpgradede gebouwen.
- Acceptatie: upgrades overleven herstart.

---

## 8. Milestones

- **Fase 0 — Prototype:** §7.1–7.6 — één turret schiet op piraten die naar het dorp gaan, in één scene.
- **Fase 1 — Vertical slice:** §7.7–7.11 — bouwen, waves met keuze, economy, UI, save. Eén speelbaar level.
- **Fase 2 — Content:** extra enemies/gebouwen via nieuwe ScriptableObjects; tutorial + main level.

Bouw niets buiten de huidige fase (scope creep).

---

## 9. Hoe ik wil dat je werkt

- **Kleine, testbare stappen.** Eén systeem per keer, dan stoppen voor mijn test.
- **Compleet en paste-ready.** Volledige bestanden, geen fragmenten.
- **Voer Editor-werk zelf uit via de Unity MCP server** (scenes, prefabs, componenten, Inspector-velden, tilemap). Geef alleen apart een blokje **"Editor-stappen"** als iets niet via MCP lukt.
- **Vraag bij twijfel.** Verzin geen ontbrekende ontwerpkeuzes.
- **Wijzig de tech stack niet** zonder overleg.

---

## 10. Bevestigde beslissingen

- [x] Werktitel: **Island Survival TD**
- [x] Kleurenpalet: vast 16-kleuren palet (§3)
- [x] Dorpje: vast startpunt, **niet koopbaar**
- [x] `fireRate` = **schoten per seconde** (turret 2 = snel, mortar 0.5 = traag)
- [x] Enemy `attackRate` = **1** schot/sec voor alle enemies

---

## 11. Startdata (uit GDD §3.3 / §3.4)

Waarden voor de `EnemyData`- en `BuildingData`-assets. Faithful overgenomen uit de GDD; startwaarden om te tunen.

### Enemies (`EnemyData`)
`attackRate` = 1 schot/sec voor alle enemies.

| Naam | maxHP | damage | coinReward | moveSpeed | range | attackRate | gedrag |
|---|---|---|---|---|---|---|---|
| Pirate boot | 10 | 1 | 5 | 1 | 1 | 1 | Spawnt, vaart naar dichtstbijzijnde hex-tile, close range |
| Fast pirate boot | 15 | 1 | 5 | 2 | 1 | 1 | idem, sneller |
| Strong pirate boot | 30 | 3 | 10 | 0.5 | 1 | 1 | idem, traag maar sterk |
| Distance pirate boot | 10 | 1 | 7 | 1 | 3 | 1 | idem, valt aan van afstand (ranged) |

### Buildings (`BuildingData`)
| Naam | maxHP | damage | cost | range | fireRate | splash | bijzonderheid |
|---|---|---|---|---|---|---|---|
| Dorpje | 100 | 5 | — (startpunt, niet koopbaar) | 3 | 1 | nee | Verdedigt zichzelf; game over als dit stuk is |
| Hexagon tile | 5 | — | 5 munten | — | — | — | Plaatsvlak voor gebouwen; koopbaar of keuze aan einde wave |
| Turret | 20 | 5 | 10 munten | 3 | 2 | nee | Schiet 1 kogel per keer op dichtstbijzijnde enemy |
| Long range turret | 20 | 4 | 15 munten | 6 | 2 | nee | Grotere range |
| Mortar (montier) | 20 | 3 | 20 munten | 8 | 0.5 | **ja** | Splash damage, lage vuursnelheid |

> Gebouwen (behalve tile/dorpje) zijn koopbaar in de hotbar. De hex-tile krijg je door te kopen of via de tussen-wave keuze (§7.8).
