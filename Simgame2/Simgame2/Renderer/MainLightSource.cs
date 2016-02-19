using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.Renderer
{
    public class MainLightSource
    {
        // Position and target of the shadowing light
        public Vector3 ShadowLightPosition { get; set; }
        public Vector3 ShadowLightTarget { get; set; }

        

        public MainLightSource(): this(Color.White, 1.0f, 0.0f, 0.0f, 0.0f) {}

        public MainLightSource(Color color, float power, float yaw, float pitch, float roll)
        {
            this.Color = color;
            this.Power = power;
            this.SetLightDirection(yaw, pitch, roll);
        }
        public Color Color { get; set; }

        public float Power { get; set; }

        public void SetLightDirection(float yaw, float pitch, float roll)
        {
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.Roll = roll;

            this.RotationMatrix = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            
            LightDirection = Vector3.Transform(Vector3.Up, this.RotationMatrix);
            LightDirection.Normalize();
        }

        public Matrix GetRotationMatrix() { return RotationMatrix; }

        public Vector3 GetLightDirection() { return this.LightDirection; }
        public Vector3 GetInvertedLightDirection() { return -this.LightDirection; }


        Vector3 LightDirection;
        public float Yaw;
        public float Pitch;
        public float Roll;
        Matrix RotationMatrix;
    }
}
