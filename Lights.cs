using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Lights
{
    public class Lights : Mod
    {
        public static bool useLight = true;

        public static bool useBloom = true;

        public static float LightIntensity = 1;

        public static bool softBlend = true;

        public static float ShadowIntensity = 1;

        public static float bloomIntensity = 1;

        public static float MoonLightIntensity = 1;

        public static bool fasttime = false;

        public RenderTarget2D screen;

        public RenderTarget2D light;

        public RenderTarget2D bloom;

        public RenderTarget2D saveImage;

        public static Effect Light;

        public static Effect Shadow;

        public static Effect Bloom;

        public static int quality = 3;

        public static int bloomCounts = 4;

        public delegate void orig_EndCapture(object self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor);

        public void ResetRenderTarget()
        {
            screen?.Dispose();
            light?.Dispose();
            bloom?.Dispose();

            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            screen = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth / 3, graphicsDevice.PresentationParameters.BackBufferHeight / 3, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            light = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            bloom = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth / 3, graphicsDevice.PresentationParameters.BackBufferHeight / 3, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

        }
        public void NewScreenTarget()
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            //Set Main.screenTarget.RenderTargetUsage to PreserveContents
            Main.screenTarget.Dispose();
            Main.screenTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

        }
        public override void Load()
        {
            //after decompiling, the original detouring here looked completely different to how it does now; I have no idea if this will cause issues or not but the mod seems to function fine
            //also, this applies to the whole file, but there may be cases where there's seemingly unneccesary casting. This may be a result of decompilation too, I just don't want to individually test each potential case for errors.
            Type vanillaDetourClass = new FilterManager().GetType();
            MethodInfo detourMethod = vanillaDetourClass.GetMethod("EndCapture", BindingFlags.Public | BindingFlags.Instance);

            On_Main.InitTargets_int_int += On_Main_InitTargets_int_int;

            //Main.OnResolutionChanged += Main_OnResolutionChanged;//fixes lighting breaking on res change
            //On_CaptureCamera.FinishCapture += CaptureCameraFinishCapture;//fixes lighting breaking on using ingame capture

            if (!Main.dedServ && detourMethod is not null)
            {
                Light = ModContent.Request<Effect>("Lights/Effects/Light", AssetRequestMode.ImmediateLoad).Value;
                Shadow = ModContent.Request<Effect>("Lights/Effects/Shadow", AssetRequestMode.ImmediateLoad).Value;
                Bloom = ModContent.Request<Effect>("Lights/Effects/Bloom1", AssetRequestMode.ImmediateLoad).Value;

                MonoModHooks.Add(detourMethod, FilterManager_EndCapture);

            }
        }

        private void On_Main_InitTargets_int_int(On_Main.orig_InitTargets_int_int orig, Main self, int width, int height)
        {
            orig(self, width, height);
            ResetRenderTarget();
            NewScreenTarget();
        }

        

        public override void Unload()
        {
            //On_FilterManager.remove_EndCapture(new hook_EndCapture(FilterManager_EndCapture));
            //Main.remove_OnResolutionChanged((Action<Vector2>)Main_OnResolutionChanged);

            //removed the above lines as there doesn't seem to be a MonoModHooks.Remove or anything to that effect, and supposedly tml automatically disables a mod's detours on unload
            Light = null;
            Shadow = null;
            Bloom = null;
            screen = null;
            light = null;
            bloom = null;
        }

        //All this CaputreInterface stuff was an attempt at fixing the bug where bloom breaks upon taking a screenshot with the ingame camera. No luck.

        /*public void CaptureInterface_Update()
        {
            screen = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 3, Main.screenHeight / 3);
            light = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            bloom = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 3, Main.screenHeight / 3);
			//orig.Invoke(self);
        }*/

        /*private void CaptureInterface_EndCamera()
        {
            screen = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 3, Main.screenHeight / 3);
            light = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            bloom = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 3, Main.screenHeight / 3);
        }*/

        /*private void CaptureCamera_FinishCapture(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            screen = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 3, Main.screenHeight / 3);
            light = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            bloom = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 3, Main.screenHeight / 3);
			//orig.Invoke(self);
        }*/

        public void FilterManager_EndCapture(orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (!CaptureManager.Instance.IsCapturing)
            {
                if (Main.screenTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents)
                {
                    NewScreenTarget();
                }
                if (screen == null)
                {
                    ResetRenderTarget();
                }
                if (useLight)
                {
                    UseLightAndShadow(graphicsDevice, spriteBatch, screenTarget1, screenTarget2);
                }
                if (useBloom)
                {
                    UseBloom(graphicsDevice, screenTarget1, screenTarget2);
                }
            }
            
            
            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }

        private void UseBloom(GraphicsDevice graphicsDevice, RenderTarget2D rt1, RenderTarget2D rt2)
        {
            if (screen.Width != rt1.Width / 3 || screen.Height != rt1.Height / 3)
            {
                ResetRenderTarget();
            }

            graphicsDevice.SetRenderTarget(rt2);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(rt1, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();


            //Get HighLights & Down Samplers
            graphicsDevice.SetRenderTarget(screen);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            Bloom.Parameters["m"].SetValue(0.71f - bloomIntensity * 0.01f);
            Bloom.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(rt1, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1 / 3f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            
            //Kawase Blur
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            Effect shader = ModContent.Request<Effect>("Lights/Effects/KawaseBlur", AssetRequestMode.ImmediateLoad).Value;
            shader.Parameters["uScreenResolution"].SetValue(new Vector2(rt1.Width, rt2.Height) / 2.5f);
            shader.Parameters["uIntensity"].SetValue(1.00f+bloomCounts*0.005f);
            for (int i = 0; i < bloomCounts; i++) 
            {
                shader.Parameters["distance"].SetValue(i);
                shader.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.SetRenderTarget(i%2==0? bloom:screen);
                graphicsDevice.Clear(Color.Black);
                Main.spriteBatch.Draw(i % 2 == 0 ? screen : bloom, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();

            //Blend Bloom to rt1

            /*
            graphicsDevice.SetRenderTarget(rt1);
            Main.spriteBatch.Begin((SpriteSortMode)1, BlendState.Additive);
            Bloom.Parameters["tex0"].SetValue((Texture)(object)rt2);
            Bloom.Parameters["p"].SetValue(3f);
            Bloom.Parameters["m2"].SetValue(bloomIntensity);
            Bloom.CurrentTechnique.Passes["Blend"].Apply();
            Main.spriteBatch.Draw((Texture2D)(object)screen, Vector2.Zero, (Rectangle?)null, Color.White, 0f, Vector2.Zero, 3f, (SpriteEffects)0, 0f);
            Main.spriteBatch.End();*/

            
            graphicsDevice.SetRenderTarget(rt1);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            Bloom.Parameters["tex0"].SetValue(rt2);
            Bloom.Parameters["p"].SetValue(4.5f);
            Bloom.Parameters["uScreenResolution"].SetValue(new Vector2(rt1.Width, rt2.Height));

            Bloom.Parameters["m2"].SetValue(bloomIntensity);
            Bloom.CurrentTechnique.Passes["Blend"].Apply();
            Main.spriteBatch.Draw(bloomCounts%2==0? screen:bloom, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 3f, 0, 0f);
            Main.spriteBatch.End();
        }

        private float Gauss(float x, float sigma)
        {
            return 0.4f * (float)Math.Exp(-0.5f * x * x / sigma);
        }

        public static void SetQuality(int q)
        {
            if (quality != q)
            {
                quality = q;
            }
        }

        private void UseLightAndShadow(GraphicsDevice gd, SpriteBatch sb, RenderTarget2D rt1, RenderTarget2D rt2)
        {
            #region Create Light in (RT2D)light
            gd.SetRenderTarget(light);
            gd.Clear(Color.Black);
            sb.Begin((SpriteSortMode)1, BlendState.NonPremultiplied);
            Light.Parameters["uScreenResolution"].SetValue(new Vector2(rt1.Width, rt1.Height));
            Light.Parameters["uPos"].SetValue(ToScreenCoords(GetSunPos(rt1), rt1));
            String fileName = GetInstance<LightsConfig>().dayColor == "Classic" ? "ColorTex" : "ColorTex_Alt";//handles changing daylight colors
            /*String fileName;//simply handles changing daylight colors
			if (GetInstance<LightsConfig>().dayColor == "Classic")
			{
				fileName = "ColorTex";
			}
			else
			{
				fileName = "ColorTex_Alt";
            }*/
            Light.Parameters["tex0"].SetValue((Texture)ModContent.Request<Texture2D>("Lights/" + (Main.dayTime ? fileName : "ColorTex2"), AssetRequestMode.AsyncLoad));
            Color colorOfTheSkies = Main.ColorOfTheSkies;
            //I have no idea what these "num" variables were originally named or what they do, the name change is likely a result of decompilation
            float num = (1f - 1.2f * colorOfTheSkies.R * 0.3f + colorOfTheSkies.G * 0.6f + colorOfTheSkies.B * 0.1f) / 255f;
            float lightIntensity = LightIntensity;
            if (Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneCrimson && !Main.LocalPlayer.ZoneCorrupt)
            {
                num -= Main.bgAlphaFrontLayer[7] * 0.1f;
            }
            if (Main.LocalPlayer.ZoneCrimson)
            {
                num += 0.3f;
            }
            if (Main.snowBG[0] == 263 || Main.snowBG[0] == 258 || Main.snowBG[0] == 267)
            {
                num -= Main.bgAlphaFrontLayer[7] * 1f;
            }
            if (Main.snowBG[0] == 263)
            {
                num -= Main.bgAlphaFrontLayer[7] * 0.5f;
            }
            if (Main.desertBG[0] == 248)
            {
                num -= Main.bgAlphaFrontLayer[2] * 0.9f;
            }
            float num2 = 1f;
            if (Main.moonPhase == 0)
            {
                num2 = 1.01f;
            }
            if (Main.moonPhase == 3 || Main.moonPhase == 5)
            {
                num2 = 0.9f;
            }
            if (Main.moonPhase == 4)
            {
                num2 = 0.6f;
            }
            float num3 = 0.8f * lightIntensity * (1f + num * 0.4f);
            if ((double)(Main.LocalPlayer.Center.Y / 16f) > Main.worldSurface - 150.0)
            {
                float num4 = Main.LocalPlayer.Center.Y / 16f - ((float)Main.worldSurface - 150f);
                num2 *= 1f - num4 / 200f;
                num3 *= 1f - num4 / 600f;
            }
            Light.Parameters["intensity"].SetValue(Main.dayTime ? num3 : (0.8f * MoonLightIntensity * num2));
            Light.Parameters["t"].SetValue((float)Main.time / 54000f);
            if (!Main.dayTime)
            {
                Light.Parameters["t"].SetValue((float)Main.time / 32400f);
            }
            Light.CurrentTechnique.Passes["Light"].Apply();
            if (Main.LocalPlayer.Center.Y < Main.worldSurface * 16.0 + 800.0)
            {
                sb.Draw((Texture2D)ModContent.Request<Texture2D>("Lights/PixelEX", AssetRequestMode.AsyncLoad), new Rectangle(0, 0, rt1.Width, rt1.Height), Color.White);
            }
            sb.End();
            #endregion
            
            #region Create Shadow in (RT2D)rt2
            gd.SetRenderTarget(screen);
            gd.Clear(Color.Black);
            sb.Begin((SpriteSortMode)1, BlendState.Additive);
            Shadow.Parameters["uScreenResolution"].SetValue(new Vector2(rt1.Width, rt1.Height));
            float num5 = Main.bgAlphaFrontLayer[2] * 0.09f;
            if (Main.desertBG[0] == 248)
            {
                num5 = 0f;
            }
            Shadow.Parameters["m"].SetValue(1.0f - num5);
            if (!Main.dayTime)
            {
                Shadow.Parameters["m"].SetValue(0.02f);
            }
            Shadow.Parameters["uPos"].SetValue(ToScreenCoords(GetSunPos(rt1), rt1));
            Shadow.CurrentTechnique.Passes[0].Apply();
            sb.Draw((Texture2D)(object)rt1, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 0.333f, 0, 0f);
            sb.End();
            

            gd.SetRenderTarget(bloom);
            gd.Clear(Color.Black);
            sb.Begin(0, BlendState.Additive);
            if (Main.dayTime)
            {
                float num6 = 120f;
                for (int i = 0; i < 30; i++)
                {
                    float num7 = 0.55f * (30 - i) / num6;
                    Color white = Color.White;
                    white.A = (byte)(white.A * num7 * (ShadowIntensity + 0.1f) * (num3 * 0.5f - 0.1f));
                    sb.Draw((Texture2D)(object)screen, GetSunPos(rt1) / 3f, null, white, 0f, GetSunPos(rt1) / 3f, 1f * (1f + i * (1f / quality) * (0.014f + quality * 0.01f)), 0, 0f);
                }
            }
            else
            {
                for (int j = 0; j < 20; j++)
                {
                    float num8 = 190f;
                    float num9 = (20f - j) / num8;
                    sb.Draw((Texture2D)(object)screen, GetSunPos(rt1) / 3f, null, Color.White * num9, 0f, GetSunPos(rt1) / 3f, 1f * (1f + j * 0.01f), 0, 0f);
                }
            }
            sb.End();
            #endregion

            #region Blend Light & Shadow into rt1
            gd.SetRenderTarget(rt1);
            sb.Begin((SpriteSortMode)1, BlendState.Additive);
            Shadow.Parameters["tex0"].SetValue((Texture)(object)bloom);
            Shadow.CurrentTechnique.Passes[1].Apply();
            sb.Draw((Texture2D)(object)light, Vector2.Zero, Color.White);
            sb.End();
            #endregion
        }

        public static Vector2 ToScreenCoords(Vector2 vec, RenderTarget2D render)
        {
            return vec / new Vector2(render.Width, render.Height);
        }

        public static Vector2 GetSunPos(RenderTarget2D render)
        {
            float num = (Main.dayTime ? 27000 : 16200);
            float num2 = (0f - Main.screenPosition.Y) / (float)(Main.worldSurface * 16.0 - 600.0) * 200f;
            float num3 = 1f - (float)Main.time / num;
            float num4 = (1f - num3) * render.Width / 2f - 100f * num3;
            float num5 = num3 * num3;
            float num6 = num2 + num5 * 250f + 180f;
            if (Main.LocalPlayer != null && Main.LocalPlayer.gravDir == -1f)
            {
                return new Vector2(num4, render.Height - num6);
            }
            return new Vector2(num4, num6);
        }

        /*public Lights()
        {
            Instance = this;
        }
        public static Lights Instance
        {
            get;
            set;
        }*/
    }
}
