﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.Renderer
{
    public class PPPointLight
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public float Attenuation { get; set; }

        public float Range { get; set; }

        public PPPointLight(Vector3 Position, Color Color, float Attenuation)
        {
            this.Position = Position;
            this.Color = Color;
            this.Attenuation = Attenuation;
            this.Range = 150;
        }

        public void SetEffectParameters(Effect effect)
        {
            effect.Parameters["LightPosition"].SetValue(Position);
            effect.Parameters["LightAttenuation"].SetValue(Attenuation);
            effect.Parameters["LightColor"].SetValue(Color.ToVector3());
        }


        public BoundingSphere GetBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(this.Position, this.Range);
            return sphere;
        }

    }
}
