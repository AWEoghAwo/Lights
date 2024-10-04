using System.ComponentModel;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader.Config;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace Lights
{
	public class LightsConfig : ModConfig
	{
        //For most of these sliders, 0.4 is around the point at which the effect becomes unnoticeable, so I capped it there.
        //also, all the { get; set; } stuff is there so each option shows up in the correct place

        public override ConfigScope Mode => ConfigScope.ClientSide;

        /*[Header("Presets")]
        [Label("Effects Presets")]
        [DrawTicks]
        [OptionStrings(new string[] { "Classic", "Subtle" })]
        [Tooltip("Classic: Default settings\nSubtle: Less intense brightness; closer to vanilla")]
        [DefaultValue("Classic")]
        public string EffectPreset 
        {
            get => lightIntensity == 1f && moonlightIntensity == 1f;
            set
            {
                if (value)
                {
                    lightIntensity = 1f;
                    moonlightIntensity = 1f;
                }
            }
        }*/

        [Header("Lighting")]

        [Label("开启光照（Use Lights）")]
		[DefaultValue(true)]
		public bool UseLight { get; set; }

        [Label("光照强度（Lights intensity）")]
        [Range(0.4f, 1.5f)]
        [DefaultValue(1f)]
		public float lightIntensity { get; set; }

        [Label("月光强度（Moon Light intensity）")] 
		[Range(0.4f, 1.1f)]
		[DefaultValue(1f)]
		public float moonlightIntensity { get; set; }

        [Label("阴影强度（Shadow intensity）")]
        [Range(0.4f, 1.5f)]
        [DefaultValue(1f)]
        public float shadowIntensity { get; set; }

        [Label("Daylight Color")]
        [DrawTicks]
        [OptionStrings(new string[] { "Classic", "Alternate" })]
        [Tooltip("Classic: Midday light is yellow\nAlternate: Midday light is white")]
        [DefaultValue("Classic")]
        public string dayColor { get; set; }

        [Header("Bloom")]

        [Label("开启外发光（Use Bloom）")]
		[DefaultValue(true)]
		public bool UseBloom { get; set; }

        [Label("外发光强度（Bloom Intensity）")]
		[Range(0.2f, 1.25f)]
		[DefaultValue(1f)]
		public float BloomIntensity { get; set; }

        [Label("Bloom迭代次数（Bloom Counts）")]
		[Range(1, 10)]
		[DefaultValue(4)]
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
			Lights.bloomCounts = BloomCounts;
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
