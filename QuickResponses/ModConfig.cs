using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace QuickResponses
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton SelectFirstResponseKey { get; set; } = SButton.G;
        public bool ShowNumbers { get; set; } = true;
        public float NumberScale { get; set; } = 0.69f;
        public Color NumberColor { get; set; } = Color.Black;
    }
}
