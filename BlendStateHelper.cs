using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lights
{
    public class BlendStateHelper
    {
        public static readonly BlendState SoftAdditive = NewBlendState("BlendState.SoftAdditive", Blend.InverseDestinationColor, Blend.SourceAlpha, Blend.One, Blend.One);

        public static BlendState NewBlendState(string name, Blend colorSourceBlend, Blend alphaSourceBlend, Blend colorDestBlend, Blend alphaDestBlend)
        {
            BlendState bs = new BlendState();

            bs.ColorSourceBlend = Blend.One;
            bs.ColorDestinationBlend = Blend.Zero;
            bs.ColorBlendFunction = BlendFunction.Add;

            bs.AlphaSourceBlend = Blend.One;
            bs.AlphaDestinationBlend = Blend.Zero;
            bs.AlphaBlendFunction = BlendFunction.Add;

            bs.ColorWriteChannels = ColorWriteChannels.All;
            bs.ColorWriteChannels1 = ColorWriteChannels.All;
            bs.ColorWriteChannels2 = ColorWriteChannels.All;
            bs.ColorWriteChannels3 = ColorWriteChannels.All;
            bs.BlendFactor = Color.White;
            bs.MultiSampleMask = -1;

            bs.Name = name;
            bs.ColorSourceBlend = colorSourceBlend;
            bs.AlphaSourceBlend = alphaSourceBlend;
            bs.ColorDestinationBlend = colorDestBlend;
            bs.AlphaDestinationBlend = alphaDestBlend;

            return bs;
        }
    }
}
