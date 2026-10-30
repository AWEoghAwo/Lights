using System.ComponentModel;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace Lights
{
	public class LightsConfig : ModConfig
	{
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Lighting")]

		[DefaultValue(true)]
		public bool UseLight { get; set; }

        [Range(0.4f, 1.2f)]
        [DefaultValue(1f)]
		public float lightIntensity { get; set; }

		[Range(0.2f, 1.1f)]
		[DefaultValue(1f)]
		public float moonlightIntensity { get; set; }

        [Range(0.6f, 1.5f)]
        [DefaultValue(1f)]
        public float shadowIntensity { get; set; }

        [DrawTicks]
        [OptionStrings(new string[] { "Classic", "Alternate" })]
        [DefaultValue("Classic")]
        public string dayColor { get; set; }

        [Header("Bloom")]
		[DefaultValue(true)]
		public bool UseBloom { get; set; }

		[Range(0.2f, 1.25f)]
		[DefaultValue(1f)]
		public float BloomIntensity { get; set; }

		[Range(1, 3)]
		[DefaultValue(2)]
		public int BloomCounts { get; set; }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
        {
            return true;
        }
        public override void OnChanged()
		{
            /*if (EffectPreset == "Classic")
            {
                UseLight = true;
                lightIntensity = 1f;
                moonlightIntensity = 1f;
                shadowIntensity = 1f;
                dayColor = "Classic";
                UseBloom = true;
                BloomIntensity = 1f;
                BloomCounts = 4;
            }
            else if (EffectPreset == "Subtle")
            {
                UseLight = true;
                lightIntensity = 0.5f;
                moonlightIntensity = 0.53f;
                shadowIntensity = 0.71f;
                dayColor = "Alternate";
                UseBloom = true;
                BloomIntensity = 0.41f;
                BloomCounts = 4;
            }*/

			if (Main.WaveQuality == 0)
			{
				Main.WaveQuality = 2;
			}
			if (Lighting.Mode == LightMode.Trippy || Lighting.Mode == LightMode.Retro)
			{
				Lighting.Mode = LightMode.Color;
            }
			Lights.LightIntensity = lightIntensity;
			Lights.useBloom = UseBloom;
			Lights.useLight = UseLight;
			Lights.ShadowIntensity = shadowIntensity;
			Lights.bloomIntensity = BloomIntensity;
			Lights.MoonLightIntensity = moonlightIntensity;
			Lights.bloomLevel = BloomCounts;
		}
        /*public LightsConfig()
        {
            Instance = this;
        }
        public static LightsConfig Instance
        {
            get;
            set;
        }*/
    }
}
