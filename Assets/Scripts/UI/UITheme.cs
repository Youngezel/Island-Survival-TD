using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Design tokens from the visual identity handoff (design_handoff_island_survival_td):
    /// the fixed color palette and layout rules shared by every screen, so
    /// screen-building code never hardcodes a hex value inline.
    /// </summary>
    public static class UITheme
    {
        // Water
        public static readonly Color DeepWater = FromHex("#0D2B40");
        public static readonly Color OpenSea = FromHex("#14496B");
        public static readonly Color WaterMid = FromHex("#2286AD");
        public static readonly Color ShallowWater = FromHex("#4FC9D9");
        public static readonly Color Foam = FromHex("#A9EEF2");

        // Land
        public static readonly Color GrassShadow = FromHex("#2F6B38");
        public static readonly Color GrassBase = FromHex("#4F9E42");
        public static readonly Color GrassLight = FromHex("#7CC35A");
        public static readonly Color GrassHighlight = FromHex("#A3D97F");
        public static readonly Color SandShadow = FromHex("#C9A063");
        public static readonly Color SandBase = FromHex("#E8C88A");
        public static readonly Color SandLight = FromHex("#DBC9A0");

        // Wood & stone
        public static readonly Color WoodDark = FromHex("#4A2E17");
        public static readonly Color WoodShadow = FromHex("#5C3A1E");
        public static readonly Color WoodPlank = FromHex("#6F4520");
        public static readonly Color WoodBase = FromHex("#8B5A2B");
        public static readonly Color WoodLight = FromHex("#B98246");
        public static readonly Color StoneDark = FromHex("#23282E");
        public static readonly Color StoneShadow = FromHex("#3F474F");
        public static readonly Color StoneMid = FromHex("#5C6672");
        public static readonly Color StoneBase = FromHex("#93A1AD");
        public static readonly Color StoneLight = FromHex("#B6C1CB");

        // UI & accents
        public static readonly Color Gold = FromHex("#FFCF3F");
        public static readonly Color GoldShadow = FromHex("#C99B1E");
        public static readonly Color Danger = FromHex("#E0503A");
        public static readonly Color DangerDark = FromHex("#B83A28");
        public static readonly Color Parchment = FromHex("#F4E4C1");
        public static readonly Color SailShadow = FromHex("#DBC9A0");
        public static readonly Color SailSeam = FromHex("#C4B18C");
        public static readonly Color PanelBackground = FromHex("#1A1420");
        public static readonly Color RowBackground = FromHex("#221A20");
        public static readonly Color SlotBackground = FromHex("#2A2028");
        public static readonly Color Divider = FromHex("#3A2A30");
        public static readonly Color TextPrimary = FromHex("#F4E4C1");
        public static readonly Color TextSecondary = FromHex("#9DB0C0");
        public static readonly Color TextDisabled = FromHex("#6F8296");
        public static readonly Color PanelBorder = FromHex("#8B5A2B");
        public static readonly Color FrameOuter = FromHex("#5C3A1E");
        public static readonly Color ButtonTextDark = FromHex("#3D2A12");

        public static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
