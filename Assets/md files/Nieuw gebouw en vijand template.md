# Nieuw gebouw / vijand — invulformulier

Vul in wat je weet, laat leeg wat je niet weet. Het "gedrag"-veld is voor dingen die niet in
een simpel getal passen (bv. een boot die zich splitst bij het sterven) — dat vraagt sowieso
losse code, dat kan altijd, beschrijf het gewoon in woorden.

Stuur dit bestand (of een kopie ervan) terug en dan bouw ik het gebouw/de vijand: data-asset,
prefab, sprite en (voor gebouwen) de upgrade-koppeling in `Shooter.cs`.

---

## 🏰 Nieuw gebouw (turret)

```
NAAM: telsa coil
BASISSTATS:
  Health: 200 (nog niet zeker)
  Damage per schot: 3
  Kosten (munten): 400
  Range (in hex-tegels): 3
  Fire rate (schoten/sec): 1
  Splash? (ja/nee):
  Splash radius (in hex-tegels, alleen als splash=ja):
  Projectiel snelheid (world units/sec):
  aantal enemies(hoeveel lightning strikes worden geschoten per schot):2
  werking van het gebouw: hij schiet electrischetijd dit raak een enemie en Ricochet of Chain lightning naar andere enemies
  ricochet(naar hoe veel enemies 1 schot door schiet): 3 enemies

PAD A (naam van het thema, bv. "bedenk iets wat hier bij past"):
  Tier 1: [meer damage] - effect:damage per shot  - waarde:5 - unlock kosten (XP):250 - koop kosten (munten):300
  Tier 2: [extra ricochet] - effect:ricochet - waarde: 6 enemies - unlock kosten (XP):1000 - koop kosten (munten):750
  Tier 3: [meer range] - effect:range - waarde:5 - unlock kosten (XP):1300 - koop kosten (munten):1200

PAD B (naam van het thema, bv. "bedenk iets wat hier bij past"):
  Tier 1: [meer schoten] - effect:aantal enemies - waarde: 5 - unlock kosten (XP):250 - koop kosten (munten):300
  Tier 2: [meer damage] - effect:damage per shot - waarde:5 - unlock kosten (XP):1000 - koop kosten (munten):800
  Tier 3: [overload] - effect: aantal enemies - waarde: 8 - unlock kosten (XP):1500 - koop kosten (munten):1400

UITERLIJK (optioneel, mag ook "verzin zelf iets passends"):
  Thema/materiaal:
  Wat maakt hem visueel anders dan de bestaande 3 turrets:
```

### Beschikbare upgrade-effecten

`effect:` moet een van deze zijn:

| Effect | Wat het doet | `waarde:` betekent |
|---|---|---|
| `Damage` | +damage per schot | extra damage (bv. 2) |
| `Range` | +bereik | extra tegels (bv. 2) |
| `FireRate` | sneller schieten | extra schoten/sec (bv. 0.3) |
| `SplashDamage` | groter explosiegebied | extra splash radius in tegels |
| `PiercingShot` | kogel gaat door vijanden heen | aantal extra vijanden geraakt |
| `SpreadShot` | schiet een fan van kogels | (waarde wordt genegeerd) |
| `SequentialDoubleShot` | schiet 2x snel achter elkaar | (waarde wordt genegeerd) |
| `MultiTargetShot` | schiet 2 vijanden tegelijk | (waarde wordt genegeerd) |
| `FireDamage` | brand-damage-over-tijd na hit | damage/sec van de burn |

⚠️ Regel: **kies per pad max 1 shot-pattern-effect** (Spread/SequentialDouble/MultiTarget) —
die sluiten elkaar uit als je ze allebei in hetzelfde pad zet, alleen de laatste geldt dan.

### Referentie (bestaande turrets, zodat je weet wat "normaal" is)

| | Turret | Mortar |
|---|---|---|
| Health | 20 | 20 |
| Damage | 5 | 3 |
| Kosten | 60 | 120 |
| Range | 3 | 8 |
| Fire rate | 2 | 0.5 |
| Splash | nee | ja, radius 1.5 |

---

## 🏴‍☠️ Nieuwe vijand (enemy)

```
NAAM:
Health:
Damage (per hit op gebouw/dorp):
Munten-beloning bij doden:
Beweegsnelheid (world units/sec):
Aanval-range (in world units - 1 = alleen van dichtbij, meer = schiet op afstand):
Aanval-snelheid (aanvallen/sec):
Projectiel snelheid (alleen relevant als range > 1):

GEDRAG (optioneel, beschrijf in woorden - bv. "duikt onder water als hij geraakt wordt",
"spawnt 2 kleintjes als hij sterft"):

UITERLIJK:
  Thema/type boot:
  Wat maakt hem visueel anders dan de bestaande 4 vijanden:
```

### Referentie (bestaande vijanden)

| | Basis | Snel | Sterk | Afstand |
|---|---|---|---|---|
| Health | 10 | 15 | 30 | 10 |
| Damage | 2 | 1 | 3 | 2 |
| Beloning | 5 | 5 | 10 | 7 |
| Snelheid | 1 | 2 | 0.5 | 1 |
| Range | 1 (melee) | 1 | 1 | 3 (afstand) |
