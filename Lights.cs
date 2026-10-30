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

        private readonly RenderTarget2D[] bloomMipTargets = new RenderTarget2D[4];

        private Effect dualBlur;

        private RenderTarget2D bloomMask;

        public RenderTarget2D shadow;

        public static Effect Light;

        public static Effect Shadow;

        public static Effect Bloom;

        public static int quality = 3;

        public static int bloomLevel = 2;

        public delegate void orig_EndCapture(object self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor);

        public void ResetRenderTarget()
        {
            screen?.Dispose();
            light?.Dispose();
            bloom?.Dispose();
            bloomMask?.Dispose();
            bloomMask = null;
            for (int i = 0; i < bloomMipTargets.Length; i++)
            {
                bloomMipTargets[i]?.Dispose();
                bloomMipTargets[i] = null;
            }
            shadow?.Dispose();
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            screen = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth / 3, graphicsDevice.PresentationParameters.BackBufferHeight / 3, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            light = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            shadow = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth / 3, graphicsDevice.PresentationParameters.BackBufferHeight / 3, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

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
                dualBlur = ModContent.Request<Effect>("Lights/Effects/DualBlur", AssetRequestMode.ImmediateLoad).Value;

                MonoModHooks.Add(detourMethod, FilterManager_EndCapture);

            }
        }

        private void On_Main_InitTargets_int_int(On_Main.orig_InitTargets_int_int orig, Main self, int width, int height)
        {
            orig(self, width, height);
            ResetRenderTarget();
        }

        

        public override void Unload()
        {   
            Light = null;
            Shadow = null;
            Bloom = null;
            dualBlur = null;
            bloom?.Dispose();
            bloomMask?.Dispose();
            bloomMask = null;
            for (int i = 0; i < bloomMipTargets.Length; i++)
            {
                bloomMipTargets[i]?.Dispose();
                bloomMipTargets[i] = null;
            }
            screen = null;
            light = null;
            bloom = null;
        }

        public void FilterManager_EndCapture(orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (!CaptureManager.Instance.IsCapturing)
            {
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

        private void UseBloom(GraphicsDevice graphicsDevice, RenderTarget2D screenTarget, RenderTarget2D screenTargetSwap)
        {
            int levels = Math.Clamp(bloomLevel, 1, bloomMipTargets.Length - 1) + 1;
            if (bloom == null || bloom.IsDisposed || bloom.Width != screenTarget.Width || bloom.Height != screenTarget.Height)
            {
                bloom?.Dispose();
                bloom = new RenderTarget2D(graphicsDevice, screenTarget.Width, screenTarget.Height, false, screenTarget.Format, DepthFormat.None);
            }
            if (bloomMask == null || bloomMask.IsDisposed || bloomMask.Width != screenTarget.Width || bloomMask.Height != screenTarget.Height)
            {
                bloomMask?.Dispose();
                bloomMask = new RenderTarget2D(graphicsDevice, screenTarget.Width, screenTarget.Height, false, screenTarget.Format, DepthFormat.None);
            }
            for (int i = 0; i < levels; i++)
            {
                int width = Math.Max(1, screenTarget.Width >> (i + 1));
                int height = Math.Max(1, screenTarget.Height >> (i + 1));
                RenderTarget2D target = bloomMipTargets[i];
                if (target == null || target.IsDisposed || target.Width != width || target.Height != height)
                {
                    target?.Dispose();
                    bloomMipTargets[i] = new RenderTarget2D(graphicsDevice, width, height, false, screenTarget.Format, DepthFormat.None);
                }
            }

            graphicsDevice.SetRenderTarget(screenTargetSwap);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            Main.spriteBatch.Draw(screenTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            // Extract highlights at the original resolution before any downsampling.
            graphicsDevice.SetRenderTarget(bloom);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            Bloom.Parameters["m"].SetValue(0.71f - bloomIntensity * 0.01f);
            Bloom.CurrentTechnique.Passes["Bloom"].Apply();
            Main.spriteBatch.Draw(screenTargetSwap, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            // Reuse each mip on the way up; its previous contents are no longer needed.
            RenderTarget2D source = bloom;
            for (int i = 0; i < levels; i++)
            {
                DrawBloomBlur(graphicsDevice, source, bloomMipTargets[i], "Downsample");
                source = bloomMipTargets[i];
            }
            for (int i = levels - 1; i >= 0; i--)
            {
                RenderTarget2D destination = i == 0 ? bloom : bloomMipTargets[i - 1];
                DrawBloomBlur(graphicsDevice, source, destination, "Upsample");
                source = destination;
            }

            // Blur only the attenuation mask; preserve the original scene for compositing.
            graphicsDevice.SetRenderTarget(bloomMask);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            Bloom.Parameters["uMaskOffset"].SetValue(new Vector2(1f / screenTargetSwap.Width, 1f / screenTargetSwap.Height));
            Bloom.CurrentTechnique.Passes["KawaseMask"].Apply();
            Main.spriteBatch.Draw(screenTargetSwap, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            graphicsDevice.SetRenderTarget(screenTarget);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            Bloom.Parameters["tex0"].SetValue(screenTargetSwap);
            Bloom.Parameters["maskTexture"].SetValue(bloomMask);
            Bloom.Parameters["p"].SetValue(4.5f);
            Bloom.Parameters["m2"].SetValue(bloomIntensity);
            Bloom.CurrentTechnique.Passes["Blend"].Apply();
            Main.spriteBatch.Draw(bloom, Vector2.Zero, Color.White);
            Main.spriteBatch.End();
        }

        private void DrawBloomBlur(GraphicsDevice graphicsDevice, RenderTarget2D source, RenderTarget2D destination, string pass)
        {
            float size = bloomLevel switch
            {
                1 => 2.5f,
                2=>2f,
                3=>1.25f,
                _=>2f
            };

            graphicsDevice.SetRenderTarget(destination);
            graphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            dualBlur.Parameters["uHalfTexelSize"].SetValue(new Vector2(size / source.Width, size / source.Height));
            dualBlur.CurrentTechnique.Passes[pass].Apply();
            Main.spriteBatch.Draw(source, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
            Main.spriteBatch.End();
        }

        public static void SetQuality(int q)
        {
            if (quality != q)
            {
                quality = q;
            }
        }

        private void UseLightAndShadow(GraphicsDevice gd, SpriteBatch sb, RenderTarget2D screenTarget, RenderTarget2D screenTargetSwap)
        {
            gd.SetRenderTarget(screenTargetSwap);
            gd.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            #region Create Light in (RT2D)light
            gd.SetRenderTarget(light);
            gd.Clear(Color.Black);
            sb.Begin((SpriteSortMode)1, BlendState.NonPremultiplied);
            Light.Parameters["uScreenResolution"].SetValue(new Vector2(screenTarget.Width, screenTarget.Height));
            Light.Parameters["uPos"].SetValue(ToScreenCoords(GetSunPos(screenTarget), screenTarget));
            String fileName = GetInstance<LightsConfig>().dayColor == "Classic" ? "ColorTex" : "ColorTex_Alt";//handles changing daylight colors
            Light.Parameters["tex0"].SetValue((Texture)ModContent.Request<Texture2D>("Lights/" + (Main.dayTime ? fileName : "ColorTex2"), AssetRequestMode.AsyncLoad));
            Color colorOfTheSkies = Main.ColorOfTheSkies;
            float num = (1f - 1.2f * colorOfTheSkies.R * 0.3f + colorOfTheSkies.G * 0.6f + colorOfTheSkies.B * 0.1f) / 255f;
            float lightIntensity = LightIntensity - 0.1f;
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
                sb.Draw((Texture2D)ModContent.Request<Texture2D>("Lights/PixelEX", AssetRequestMode.AsyncLoad), new Rectangle(0, 0, screenTarget.Width, screenTarget.Height), Color.White);
            }
            sb.End();
            #endregion
            
            #region Create Shadow in (RT2D)rt2
            gd.SetRenderTarget(screen);
            gd.Clear(Color.Black);
            sb.Begin((SpriteSortMode)1, BlendState.Additive);
            Shadow.Parameters["uScreenResolution"].SetValue(new Vector2(screenTarget.Width, screenTarget.Height));
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
            Shadow.Parameters["uPos"].SetValue(ToScreenCoords(GetSunPos(screenTarget), screenTarget));
            Shadow.CurrentTechnique.Passes[0].Apply();
            sb.Draw(screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 0.333f, 0, 0f);
            sb.End();
            
            gd.SetRenderTarget(shadow);
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
                    sb.Draw(screen, GetSunPos(screenTarget) / 3f, null, white, 0f, GetSunPos(screenTarget) / 3f, 1f * (1f + i * (1f / quality) * (0.012f + quality * 0.01f)), 0, 0f);
                }
            }
            else
            {
                for (int j = 0; j < 20; j++)
                {
                    float num8 = 190f;
                    float num9 = (20f - j) / num8;
                    sb.Draw(screen, GetSunPos(screenTarget) / 3f, null, Color.White * num9, 0f, GetSunPos(screenTarget) / 3f, 1f * (1f + j * 0.01f), 0, 0f);
                }
            }
            sb.End();
            #endregion

            #region Blend Light & Shadow into rt1
            gd.SetRenderTarget(screenTarget);
            gd.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(screenTargetSwap, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            sb.Begin((SpriteSortMode)1, BlendState.Additive);
            Shadow.Parameters["tex0"].SetValue((Texture)(object)shadow);
            Shadow.CurrentTechnique.Passes[1].Apply();
            sb.Draw(light, Vector2.Zero, Color.White);
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
    }
}
