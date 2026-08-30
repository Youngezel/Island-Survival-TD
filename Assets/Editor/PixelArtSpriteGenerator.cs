using System.IO;
using UnityEngine;
using UnityEditor;

namespace Game.EditorTools
{
    /// <summary>
    /// Draws the game's pixel-art sprites (enemies, buildings, hex tile) as
    /// literal rectangles of color, following the part sizes and palette
    /// from the visual identity handoff (Assets/md files/Island Survival TD
    /// visueel/). Menu-driven one-shot generator, not runtime code.
    /// </summary>
    public static class PixelArtSpriteGenerator
    {
        private const string OutputDir = "Assets/Art/Sprites";

        private static readonly Color WoodDark = Hex("#4A2E17");
        private static readonly Color WoodShadow = Hex("#5C3A1E");
        private static readonly Color WoodPlank = Hex("#6F4520");
        private static readonly Color WoodBase = Hex("#8B5A2B");
        private static readonly Color WoodLight = Hex("#B98246");
        private static readonly Color StoneDark = Hex("#23282E");
        private static readonly Color StoneShadow = Hex("#3F474F");
        private static readonly Color StoneMid = Hex("#5C6672");
        private static readonly Color StoneBase = Hex("#93A1AD");
        private static readonly Color StoneLight = Hex("#B6C1CB");
        private static readonly Color Gold = Hex("#FFCF3F");
        private static readonly Color Danger = Hex("#E0503A");
        private static readonly Color DangerDark = Hex("#B83A28");
        private static readonly Color Parchment = Hex("#F4E4C1");
        private static readonly Color SailShadow = Hex("#DBC9A0");
        private static readonly Color SailSeam = Hex("#C4B18C");
        private static readonly Color PanelBackground = Hex("#1A1420");
        private static readonly Color Foam = Hex("#A9EEF2");
        private static readonly Color ShallowWater = Hex("#4FC9D9");
        private static readonly Color SandBase = Hex("#E8C88A");
        private static readonly Color SandShadow = Hex("#C9A063");
        private static readonly Color GrassShadow = Hex("#2F6B38");
        private static readonly Color GrassBase = Hex("#4F9E42");
        private static readonly Color GrassLight = Hex("#7CC35A");
        private static readonly Color GrassHighlight = Hex("#A3D97F");
        private static readonly Color CrewSkin = Hex("#E8C88A");

        [MenuItem("Island Survival TD/Generate Pixel Art Sprites")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(OutputDir);

            SaveUnitSprite("spr_pirate_boat", DrawPirateBoat());
            SaveUnitSprite("spr_pirate_boat_fast", DrawFastPirateBoat());
            SaveUnitSprite("spr_pirate_boat_strong", DrawStrongPirateBoat());
            SaveUnitSprite("spr_pirate_boat_distance", DrawDistancePirateBoat());
            SaveUnitSprite("spr_village", DrawVillage());
            SaveUnitSprite("spr_turret", DrawTurret());
            SaveUnitSprite("spr_turret_long_range", DrawLongRangeTurret());
            SaveUnitSprite("spr_turret_mortar", DrawMortar());
            SaveTileSprite("spr_tile_grass_1", DrawHexTileGrass(1));
            SaveTileSprite("spr_tile_grass_2", DrawHexTileGrass(2));
            SaveTileSprite("spr_tile_grass_3", DrawHexTileGrass(3));
            SaveTileSprite("spr_tile_coast_skirt", DrawCoastSkirt());
            SaveSprite("spr_explosion", DrawExplosion(), pivotBottomCenter: false, ppu: 32);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Pixel art sprites generated in " + OutputDir);
        }

        // ---------------------------------------------------------------
        // Enemies (32x32)
        // ---------------------------------------------------------------

        private static PixelCanvas DrawPirateBoat()
        {
            var c = new PixelCanvas(32);
            int hullY = 20;

            c.Trapezoid(7, hullY, 17, 13, 5, WoodBase);
            c.Rect(7, hullY, 17, 1, WoodLight); // top rail highlight
            c.Rect(8, hullY + 2, 15, 1, WoodPlank); // plank line
            c.Rect(9, hullY + 4, 11, 1, WoodShadow); // dark bottom edge
            c.Rect(23, hullY + 1, 2, 3, WoodShadow); // rudder

            c.Rect(15, 7, 2, 13, WoodShadow); // mast
            c.Rect(10, 7, 12, 1, WoodBase); // yard

            c.Rect(11, 8, 9, 9, Parchment); // sail
            c.Rect(11, 11, 9, 2, Danger); // stripe
            c.Rect(18, 8, 2, 9, SailShadow); // right shadow edge

            c.Rect(16, 4, 1, 3, WoodShadow); // flagpole above mast
            c.Rect(17, 4, 5, 2, Danger); // flag

            c.Rect(12, hullY - 2, 2, 2, CrewSkin); // crew dot
            c.Rect(12, hullY - 2, 2, 1, Danger); // bandana

            c.Rect(20, hullY + 1, 3, 2, WoodLight); // barrel on deck

            c.Rect(3, hullY + 4, 7, 1, Foam);
            c.Rect(0, hullY + 5, 11, 1, Foam);

            c.HealthBarAnchor(16, 3);
            return c;
        }

        private static PixelCanvas DrawFastPirateBoat()
        {
            var c = new PixelCanvas(32);
            int hullY = 22;

            c.Trapezoid(3, hullY, 26, 20, 3, SailShadow);
            c.Rect(3, hullY, 26, 1, Parchment); // light rail
            c.Rect(6, hullY + 2, 20, 1, WoodBase); // bottom edge

            c.Rect(0, hullY, 4, 1, WoodShadow); // bowsprit forward
            c.Rect(27, hullY + 1, 5, 1, WoodShadow); // oar aft

            c.Rect(15, 4, 2, 18, WoodShadow); // tall thin mast

            // raked triangular main sail (tapers toward the top)
            c.TrapezoidUp(17, 5, 2, 10, 13, Parchment);
            c.TrapezoidUpHalf(17, 5, 2, 10, 13, SailShadow, true);

            // smaller foresail, mirrored triangle forward
            c.TrapezoidUp(6, 10, 2, 6, 8, Parchment);

            c.Rect(16, 1, 4, 2, Danger); // flag

            c.Rect(2, hullY + 2, 12, 2, Foam);
            c.Rect(14, hullY + 3, 8, 1, ShallowWater);

            c.HealthBarAnchor(16, 2);
            return c;
        }

        private static PixelCanvas DrawStrongPirateBoat()
        {
            var c = new PixelCanvas(32);
            int hullY = 19;

            c.Rect(2, hullY, 27, 8, StoneMid);
            c.Trapezoid(2, hullY, 27, 27, 8, WoodBase);
            c.Rect(2, hullY, 27, 1, WoodLight);
            c.Rect(3, hullY + 3, 25, 1, WoodDark);
            c.Rect(3, hullY + 6, 25, 1, WoodDark);

            // 4 steel plates evenly spread
            for (int i = 0; i < 4; i++)
            {
                int px = 4 + i * 6;
                c.Rect(px, hullY + 2, 4, 4, StoneBase);
                c.Rect(px, hullY + 5, 4, 1, StoneMid);
            }

            c.Rect(15, 6, 3, 13, WoodShadow); // mast
            c.Rect(12, 5, 7, 3, WoodBase); // crow's nest
            c.Rect(12, 7, 7, 1, WoodShadow);

            c.Rect(8, 8, 16, 10, SailShadow); // double sail
            c.Rect(12, 8, 1, 10, SailSeam);
            c.Rect(20, 8, 1, 10, SailSeam);
            c.Rect(14, 11, 3, 3, Parchment); // skull
            c.Rect(14, 12, 1, 1, PanelBackground);
            c.Rect(16, 12, 1, 1, PanelBackground);

            c.Rect(16, 2, 6, 2, PanelBackground); // black flag

            c.Rect(0, hullY + 6, 30, 2, Foam);
            c.Rect(3, hullY + 8, 24, 1, ShallowWater);

            c.HealthBarAnchor(16, 1);
            return c;
        }

        private static PixelCanvas DrawDistancePirateBoat()
        {
            var c = new PixelCanvas(32);
            int hullY = 20;

            c.Trapezoid(6, hullY, 19, 15, 5, WoodBase);
            c.Rect(6, hullY, 19, 1, WoodLight);
            c.Rect(7, hullY + 4, 13, 1, WoodShadow);

            // cannon carriage + barrel sticking out past the hull silhouette
            c.Rect(6, hullY - 5, 6, 6, StoneBase);
            c.Rect(6, hullY - 1, 6, 1, StoneMid);
            c.Rect(0, hullY - 4, 10, 3, StoneMid); // barrel protrudes left, past the hull
            c.Rect(0, hullY - 4, 2, 3, StoneBase); // muzzle ring
            c.Rect(3, hullY - 4, 1, 3, StoneDark); // barrel band
            c.Rect(6, hullY - 4, 1, 3, StoneDark); // barrel band

            c.Rect(21, 9, 2, 11, WoodShadow); // mast aft
            c.Rect(20, 10, 6, 7, Parchment);
            c.Rect(20, 13, 6, 1, Danger);

            c.Rect(22, hullY + 1, 3, 3, WoodLight); // powder barrel
            c.Rect(15, hullY - 2, 2, 2, CrewSkin); // crew dot

            c.Rect(20, 6, 5, 2, PanelBackground); // black flag

            c.Rect(4, hullY + 4, 8, 1, Foam);

            c.HealthBarAnchor(16, 3);
            return c;
        }

        // ---------------------------------------------------------------
        // Buildings (32x32)
        // ---------------------------------------------------------------

        private static PixelCanvas DrawVillage()
        {
            var c = new PixelCanvas(32);

            c.Rect(2, 27, 4, 2, WoodLight); // crate left
            c.Rect(26, 28, 4, 2, WoodLight); // crate right

            c.Rect(6, 20, 20, 4, WoodBase); // sokkel
            c.Rect(6, 23, 20, 1, WoodShadow);

            c.Rect(6, 10, 20, 10, WoodBase); // romp
            c.Rect(6, 10, 20, 1, SailShadow);
            c.Rect(7, 15, 18, 1, WoodPlank);

            c.Rect(14, 13, 4, 6, WoodShadow); // deur
            c.Rect(17, 15, 1, 1, Gold); // klink

            c.Rect(8, 12, 4, 3, ShallowWater); // raam links
            c.Rect(9, 13, 1, 2, WoodShadow);
            c.Rect(9, 12, 2, 1, WoodShadow);
            c.Rect(20, 12, 4, 3, ShallowWater); // raam rechts
            c.Rect(21, 13, 1, 2, WoodShadow);
            c.Rect(21, 12, 2, 1, WoodShadow);

            c.Trapezoid(4, 4, 24, 4, 8, Danger); // dak driehoek-achtig (breed onder, smal boven)
            c.Rect(4, 7, 24, 1, DangerDark);
            c.Rect(4, 10, 24, 1, DangerDark);
            c.Rect(3, 11, 25, 1, WoodBase); // dakrand met overstek

            c.Rect(24, 3, 3, 6, StoneBase); // schoorsteen
            c.Rect(24, 3, 3, 1, StoneMid);
            c.Rect(23, 1, 2, 2, new Color(0.86f, 0.89f, 0.92f, 0.7f)); // rook
            c.Rect(21, 0, 3, 1, new Color(0.86f, 0.89f, 0.92f, 0.45f));

            c.Rect(9, 0, 2, 9, WoodShadow); // vlaggenmast
            c.Rect(11, 1, 6, 4, Gold); // wimpel
            c.Rect(9, 3, 4, 1, WoodShadow); // dwarsbalk

            c.HealthBarAnchor(16, -3);
            return c;
        }

        private static PixelCanvas DrawTurret()
        {
            var c = new PixelCanvas(32);

            c.Rect(4, 18, 24, 14, StoneBase); // voetstuk
            c.Rect(4, 18, 24, 2, StoneLight);
            c.Rect(4, 22, 24, 1, StoneMid);
            c.Rect(15, 18, 1, 14, StoneMid);
            c.Rect(4, 30, 24, 4, StoneMid); // sokkel

            c.Rect(3, 20, 7, 6, WoodLight); // zandzak/vat links
            c.Rect(22, 21, 6, 5, WoodBase); // kist rechts
            c.Rect(9, 24, 4, 4, StoneShadow); // luik
            c.Rect(19, 24, 4, 4, Danger); // rode markering

            c.Rect(8, 8, 16, 10, WoodBase); // geschutkoepel
            c.Rect(8, 8, 16, 1, WoodLight);
            c.Rect(8, 17, 16, 1, WoodShadow);

            c.Rect(6, 11, 14, 4, StoneMid); // loop
            c.Rect(6, 11, 14, 1, StoneLight);
            c.Rect(3, 10, 3, 6, StoneBase); // mondring

            c.HealthBarAnchor(16, 5);
            return c;
        }

        private static PixelCanvas DrawLongRangeTurret()
        {
            var c = new PixelCanvas(32);

            c.Rect(4, 26, 24, 6, StoneMid); // voet
            c.Rect(13, 21, 6, 5, WoodBase); // vat naast de voet

            c.Rect(10, 10, 12, 22, StoneBase); // schacht
            c.Rect(10, 10, 3, 22, StoneLight);
            c.Rect(19, 10, 3, 22, StoneMid);
            c.Rect(10, 16, 12, 1, StoneMid);
            c.Rect(10, 23, 12, 1, StoneMid);
            c.Rect(14, 14, 4, 5, PanelBackground); // kijkgat

            c.Rect(6, 8, 20, 8, WoodBase); // uitkijkplatform
            c.Rect(6, 8, 20, 1, WoodLight);
            c.Rect(6, 15, 20, 1, WoodShadow);

            c.TrapezoidUp(12, 0, 0, 8, 6, Parchment); // spits dak
            c.TrapezoidUpHalf(12, 0, 0, 8, 6, SailShadow, true);
            c.Rect(15, 0, 1, 2, WoodShadow); // vlaggenmast-tip
            c.Rect(16, 0, 4, 2, Danger); // vlaggetje

            c.Rect(2, 10, 2, 18, WoodBase); // ladder staander
            for (int i = 0; i < 3; i++)
            {
                c.Rect(2, 13 + i * 5, 6, 1, WoodLight); // sporten
            }

            c.Rect(11, 18, 10, 3, StoneMid); // loop horizontaal

            c.HealthBarAnchor(16, 0);
            return c;
        }

        private static PixelCanvas DrawMortar()
        {
            var c = new PixelCanvas(32);

            c.Rect(3, 20, 26, 12, StoneMid); // basis
            c.Rect(3, 20, 26, 2, StoneBase);
            c.Rect(3, 24, 26, 1, StoneShadow);
            c.Rect(15, 20, 1, 12, StoneShadow);
            c.Rect(3, 32, 26, 4, StoneShadow); // sokkel

            c.Trapezoid(8, 10, 16, 8, 10, StoneBase); // trechtermond (breed onder, smal boven)
            c.Rect(8, 14, 8, 10, StoneLight); // linkerhelft highlight
            c.Rect(8, 17, 16, 1, StoneMid); // naad

            c.Rect(12, 20, 8, 6, StoneShadow); // kamer
            c.Rect(13, 20, 6, 2, StoneDark); // opening

            c.Rect(15, 3, 3, 3, Gold); // granaat
            c.Rect(11, 6, 2, 2, new Color(Gold.r, Gold.g, Gold.b, 0.5f));
            c.Rect(20, 6, 2, 2, new Color(Gold.r, Gold.g, Gold.b, 0.5f));

            c.Circle(6, 29, 2, StoneShadow);
            c.Circle(16, 30, 2, StoneDark);
            c.Circle(26, 29, 2, StoneShadow);
            c.Rect(2, 24, 3, 8, WoodBase); // steunblok links
            c.Rect(27, 24, 3, 8, WoodBase); // steunblok rechts

            c.HealthBarAnchor(16, 1);
            return c;
        }

        // ---------------------------------------------------------------
        // Effects
        // ---------------------------------------------------------------

        /// <summary>
        /// A radial burst used for impact VFX. Drawn centered so it can be
        /// scaled up/down uniformly (small fixed hit-spark for the turret
        /// and long-range turret, or stretched to match the mortar's actual
        /// splash radius) without needing separate art per size.
        /// </summary>
        private static PixelCanvas DrawExplosion()
        {
            var c = new PixelCanvas(32);
            c.Circle(16, 16, 15, new Color(DangerDark.r, DangerDark.g, DangerDark.b, 0.6f));
            c.Circle(16, 16, 12, DangerDark);
            c.Circle(16, 16, 9, Danger);
            c.Circle(16, 16, 6, Gold);
            c.Circle(16, 16, 3, Parchment);
            return c;
        }

        // ---------------------------------------------------------------
        // Environment (64x64 hex tile)
        // ---------------------------------------------------------------

        // The real hex-grid cell is 2.0 world units wide (corner-to-corner)
        // by ~1.732 tall (flat-to-flat) - a 64x56 canvas matches that ratio
        // at 32 PPU, so tiles butt against each other with no gaps.
        private const int TileWidth = 64;
        private const int TileHeight = 56;

        /// <summary>
        /// A pure-grass tile with no sand or water pixels at all, for any
        /// cell that doesn't border open water. Detail props (flowers, a
        /// rock, a palm) are picked from a fixed pool using <paramref
        /// name="seed"/> so a handful of distinct-looking variants exist to
        /// choose between at placement time.
        /// </summary>
        private static PixelCanvas DrawHexTileGrass(int seed)
        {
            var c = new PixelCanvas(TileWidth, TileHeight);

            for (int y = 0; y < TileHeight; y++)
            {
                for (int x = 0; x < TileWidth; x++)
                {
                    if (!c.InsideFlatTopHex(x, y)) continue;
                    c.SetRaw(x, y, GrassBase);
                }
            }

            var detailPool = new (int x, int y, int w, int h, Color color)[]
            {
                (20, 8, 3, 2, GrassLight),
                (40, 12, 3, 2, GrassShadow),
                (10, 16, 2, 2, GrassHighlight),
                (28, 6, 2, 2, Gold), // gele bloem
                (36, 24, 2, 2, Danger), // rode bloem
                (46, 20, 6, 4, StoneBase), // rots
                (46, 23, 6, 1, StoneMid),
                (18, 28, 2, 2, GrassLight),
                (34, 30, 2, 2, GrassHighlight),
            };

            var rng = new System.Random(seed);
            var order = new System.Collections.Generic.List<int>();
            for (int i = 0; i < detailPool.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }

            int detailCount = Mathf.Min(4, order.Count);
            for (int i = 0; i < detailCount; i++)
            {
                var d = detailPool[order[i]];
                c.Rect(d.x, d.y, d.w, d.h, d.color);
            }

            if (seed % 2 == 0)
            {
                c.Rect(16, 26, 2, 8, WoodBase); // palmstam
                c.Rect(12, 22, 5, 2, GrassShadow);
                c.Rect(17, 20, 8, 2, GrassBase);
                c.Rect(14, 24, 6, 2, GrassLight);
            }

            return c;
        }

        /// <summary>
        /// A sand hex drawn oversized relative to the true tile footprint,
        /// painted on a separate tilemap layer underneath the grass so it
        /// pokes out past a land cell's own edge. A neighboring land cell's
        /// grass sprite - sized exactly to its own cell - covers whatever
        /// portion of this overflow lands inside it, so sand only ever
        /// shows through on edges that actually face open water. This
        /// avoids baking a full sand ring into every coastal tile (which
        /// showed sand even on edges touching another land tile).
        /// </summary>
        private static PixelCanvas DrawCoastSkirt()
        {
            const int skirtWidth = 80; // 1.25x TileWidth/TileHeight, so it overflows past each cell's true edge
            const int skirtHeight = 70;

            var c = new PixelCanvas(skirtWidth, skirtHeight);

            for (int y = 0; y < skirtHeight; y++)
            {
                for (int x = 0; x < skirtWidth; x++)
                {
                    if (!c.InsideFlatTopHex(x, y)) continue;
                    Color color = y < skirtHeight * 0.6f ? SandBase : SandShadow;
                    c.SetRaw(x, y, color);
                }
            }

            return c;
        }

        // ---------------------------------------------------------------
        // Import + save helpers
        // ---------------------------------------------------------------

        private static void SaveUnitSprite(string name, PixelCanvas canvas)
        {
            SaveSprite(name, canvas, pivotBottomCenter: true, ppu: 32);
        }

        private static void SaveTileSprite(string name, PixelCanvas canvas)
        {
            SaveSprite(name, canvas, pivotBottomCenter: false, ppu: 32);
        }

        private static void SaveSprite(string name, PixelCanvas canvas, bool pivotBottomCenter, int ppu)
        {
            string path = $"{OutputDir}/{name}.png";
            File.WriteAllBytes(path, canvas.ToTexture().EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            var spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            spriteSettings.spriteAlignment = (int)(pivotBottomCenter ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
            spriteSettings.spritePivot = pivotBottomCenter ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(spriteSettings);

            importer.SaveAndReimport();
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        /// <summary>
        /// Pixel grid addressed with (0,0) at the TOP-LEFT and y growing
        /// downward, matching how the handoff describes every sprite part -
        /// flips to Unity's bottom-left texture space only when writing.
        /// </summary>
        private class PixelCanvas
        {
            private readonly Color[] _pixels;
            private readonly int _width;
            private readonly int _height;

            public PixelCanvas(int size) : this(size, size)
            {
            }

            public PixelCanvas(int width, int height)
            {
                _width = width;
                _height = height;
                _pixels = new Color[width * height];
            }

            public void Rect(int x, int y, int w, int h, Color color)
            {
                for (int dy = 0; dy < h; dy++)
                {
                    for (int dx = 0; dx < w; dx++)
                    {
                        SetTopLeft(x + dx, y + dy, color);
                    }
                }
            }

            /// <summary>A rect whose top row is wTop wide and bottom row is wBottom wide, both centered on the same x.</summary>
            public void Trapezoid(int x, int y, int wTop, int wBottom, int h, Color color)
            {
                int centerX = x + wTop / 2;
                for (int row = 0; row < h; row++)
                {
                    float t = h <= 1 ? 0f : (float)row / (h - 1);
                    int w = Mathf.RoundToInt(Mathf.Lerp(wTop, wBottom, t));
                    int rowX = centerX - w / 2;
                    Rect(rowX, y + row, w, 1, color);
                }
            }

            /// <summary>Same as Trapezoid but widens going DOWN (used for sails/roofs that taper to a point at the top).</summary>
            public void TrapezoidUp(int x, int y, int wTop, int wBottom, int h, Color color)
            {
                Trapezoid(x, y, wTop, wBottom, h, color);
            }

            /// <summary>Shades the right (or left) half of a TrapezoidUp shape to fake a shadow side.</summary>
            public void TrapezoidUpHalf(int x, int y, int wTop, int wBottom, int h, Color color, bool rightHalf)
            {
                int centerX = x + wTop / 2;
                for (int row = 0; row < h; row++)
                {
                    float t = h <= 1 ? 0f : (float)row / (h - 1);
                    int w = Mathf.RoundToInt(Mathf.Lerp(wTop, wBottom, t));
                    int rowX = centerX - w / 2;
                    int half = Mathf.Max(1, w / 2);
                    if (rightHalf)
                    {
                        Rect(rowX + w - half, y + row, half, 1, color);
                    }
                    else
                    {
                        Rect(rowX, y + row, half, 1, color);
                    }
                }
            }

            public void Circle(int cx, int cy, int r, Color color)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (dx * dx + dy * dy <= r * r + 1)
                        {
                            SetTopLeft(cx + dx, cy + dy, color);
                        }
                    }
                }
            }

            /// <summary>No-op marker documenting where a runtime-drawn health bar should be anchored (handled by a UI overlay, not baked into the sprite).</summary>
            public void HealthBarAnchor(int x, int y)
            {
            }

            /// <summary>
            /// True if (x,y) lies inside a flat-top hex that exactly fills this
            /// canvas (width = point-to-point, height = flat-to-flat, matching
            /// the real grid's 2.0 : 1.732 cell proportions). scale &lt; 1 tests
            /// against a smaller hex shrunk toward the center, for insetting a
            /// border (e.g. a coastal sand ring) that follows the true hex edge.
            /// </summary>
            public bool InsideFlatTopHex(int x, int y, float scale = 1f)
            {
                float nx = x / (float)_width;
                float ny = y / (float)_height;

                if (scale < 1f)
                {
                    nx = 0.5f + (nx - 0.5f) / scale;
                    ny = 0.5f + (ny - 0.5f) / scale;
                    if (nx < 0f || nx > 1f || ny < 0f || ny > 1f)
                    {
                        return false;
                    }
                }

                if (nx < 0.25f)
                {
                    float edgeY0 = 0.5f, edgeY1 = 0f;
                    float t = nx / 0.25f;
                    float topLimit = Mathf.Lerp(edgeY0, edgeY1, t);
                    float bottomLimit = 1f - topLimit;
                    return ny >= topLimit && ny <= bottomLimit;
                }

                if (nx > 0.75f)
                {
                    float t = (nx - 0.75f) / 0.25f;
                    float topLimit = Mathf.Lerp(0f, 0.5f, t);
                    float bottomLimit = 1f - topLimit;
                    return ny >= topLimit && ny <= bottomLimit;
                }

                return true;
            }

            public void SetRaw(int x, int y, Color color) => SetTopLeft(x, y, color);

            private void SetTopLeft(int x, int y, Color color)
            {
                int py = (_height - 1) - y;
                if (x < 0 || x >= _width || py < 0 || py >= _height)
                {
                    return;
                }

                _pixels[py * _width + x] = color;
            }

            public Texture2D ToTexture()
            {
                var tex = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                tex.SetPixels(_pixels);
                tex.Apply();
                return tex;
            }
        }
    }
}
