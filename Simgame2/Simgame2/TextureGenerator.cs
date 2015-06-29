using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Simgame2
{
    public class TextureGenerator : Microsoft.Xna.Framework.GameComponent
    {
        // source http://lodev.org/cgtutor/randomnoise.html


        public TextureGenerator(Game game)
            : base(game)
        {
            this.device = game.GraphicsDevice;
        }

        public GraphicsDevice device { get; set; }

        public override void Initialize()
        {


            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }




        public Texture2D GenerateGroundTexture(Color maxColor, int size)
        {
            return CreateStaticMap(maxColor, size);
        }

        float scale = 0.006f;
        double[,] noise;
        void generateNoise(int resolution)
        {
            Random rand = new Random();
            noise = new double[resolution,resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                {
                    noise[x, y] = (rand.Next(32768)) / 32768.0 * scale;
                }
        }

        private Texture2D CreateStaticMap(Color maxColor, int resolution)
        {
            generateNoise(resolution);

            Color[] noisyColors = new Color[resolution * resolution];
            float r, g, b;
            double randomValue;
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                {
                    randomValue = (turbulence(x, y, 64, resolution));

                    r = (float)(randomValue * (maxColor.R / 2));
                    b = (float)(randomValue * (maxColor.B / 2));
                    g = (float)(randomValue * (maxColor.G / 2));

                    noisyColors[x + y * resolution] = new Color(r, g, b);
                }  


            Texture2D noiseImage = new Texture2D(device, resolution, resolution, false, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        double smoothNoise(double x, double y, int resolution)
        {
            //get fractional part of x and y
            double fractX = x - (int)(x);
            double fractY = y - (int)(y);

            //wrap around
            int x1 = ((int)(x) + resolution) % resolution;
            int y1 = ((int)(y) + resolution) % resolution;

            //neighbor values
            int x2 = (x1 + resolution - 1) % resolution;
            int y2 = (y1 + resolution - 1) % resolution;

            //smooth the noise with bilinear interpolation
            double value = 0.0;
            value += fractX * fractY * noise[x1, y1];
            value += fractX * (1 - fractY) * noise[x1, y2];
            value += (1 - fractX) * fractY * noise[x2, y1];
            value += (1 - fractX) * (1 - fractY) * noise[x2, y2];

            return value;
        }


        double turbulence(double x, double y, double size, int resolution)
        {
            double value = 0.0, initialSize = size;

            while (size >= 1)
            {
                value += smoothNoise(x / size, y / size, resolution) * size;
                size /= 2.0;
            }

            return (value / initialSize);
        }


    }
}
