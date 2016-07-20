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
    public class TextureGenerator
    {
        // source http://lodev.org/cgtutor/randomnoise.html

        protected GameSession.GameSession RunningGameSession;

        public TextureGenerator(GameSession.GameSession RunningGameSession)
        {
            this.device = RunningGameSession.device;
            this.RunningGameSession = RunningGameSession;
        }

        public GraphicsDevice device { get; set; }

        public virtual void Initialize()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
        }


        public Texture2D baseTexture { get; set; }

        public Texture2D GenerateGroundTexture(Color maxColor, Vector3 margin, int size)
        {
            Texture2D text;
            text = CreateStaticMap(maxColor, margin, size);

            //text = GroundImage(maxColor, margin, size);


            return text;
        }



        private Texture2D GroundImage(Color maxColor, Vector3 margin, int size)
        {
            Color[] noisyColors = new Color[size * size];
            int r, g, b;


            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float value = (float)((1 + Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) / 2);


                    r = (int)(maxColor.R - (value * margin.X));
                    b = (int)(maxColor.B - (value * margin.Y));
                    g = (int)(maxColor.G - (value * margin.Z));

                    // rbg must be int (range 0-255) or floats (range 0-1);
                    noisyColors[x + y * size] = new Color(r, g, b);

                }
            }


            Texture2D noiseImage = new Texture2D(device, size, size, false, SurfaceFormat.Color);
           
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }




        public Texture2D SelectionImage(Color color, int size)
        {
            Color[] colors = new Color[size * size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    colors[x + y * size] = color;

                }
            }


            Texture2D selectionImage = new Texture2D(device, size, size, false, SurfaceFormat.Color);

            selectionImage.SetData(colors);
            return selectionImage;
        }






        public Vector2[] GenerateBumpMap(Texture2D inputImage)
        {
            int size = inputImage.Width * inputImage.Height;
            Color[] colors = new Color[size]; 
            inputImage.GetData<Color>(colors);


            // obtain brightness of the colors (HSV, but only the V-component)
            int[] V = new int[size];
            for (int i = 0; i < size; i++)
            {
                V[i] = Max(colors[i].R, colors[i].G, colors[i].B);
            }

            // first derivative
            int[] dVx = new int[inputImage.Width * inputImage.Height];
            int adress;
            for (int y = 0; y < inputImage.Height; y++)
            {
                dVx[inputImage.Width * y] = 0;
                for (int x = 1; x < inputImage.Width-1; x++)
                {
                    adress = x + inputImage.Width * y;
                    dVx[adress] = V[adress+1] - V[adress];
                }
            }

            int[] dVy = new int[inputImage.Width * inputImage.Height];
            for (int x = 0; x < inputImage.Width; x++)
            {
                dVy[x] = 0;
                for (int y = 1; y < inputImage.Height - 1; y++)
                {
                    adress = x + inputImage.Width * y;
                    dVy[adress] = V[adress + inputImage.Width * y] - V[adress];
                }
            }



            Vector2[] bumps = new Vector2[size];
            for (int x = 0; x < inputImage.Width; x++)
            {
                for (int y = 0; y < inputImage.Height; y++)
                {
                    adress = x + inputImage.Width * y;
                    bumps[adress] = new Vector2(dVx[adress], dVy[adress]);
                }
            }


            return bumps;

        }


        private int Max(int a, int b, int c)
        {
            if (a > b)
            {
                if (a > c)
                {
                    return a;
                }
                else
                {
                    return c;
                }
            }
            else
            {
                if (b > c)
                {
                    return b;
                }
                else
                {
                    return c;
                }
            }
        }


    //    float scale = 0.006f;
        double[,] noise;
        void generateNoise(int resolution)
        {
            Random rand = new Random();
            noise = new double[resolution,resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                {
                    //noise[x, y] = (rand.Next(32768)) / 32768.0 * scale;
                    noise[x, y] = SimplexNoise.Noise.Generate(x, y);

                }
        }

        private Texture2D CreateStaticMap(Color maxColor, Vector3 margin, int resolution)
        {
            generateNoise(resolution);

            Color[] noisyColors = new Color[resolution * resolution];
            int r, g, b;
            double randomValue;
            for (int x = 0; x < resolution/2; x++)
                for (int y = 0; y < resolution/2; y++)
                {
                    //randomValue = (turbulence(x, y, 512, resolution));
                    randomValue = smoothNoise(x, y, resolution);

                    r = (int)(maxColor.R - (randomValue * margin.X));
                    b = (int)(maxColor.B - (randomValue * margin.Y));
                    g = (int)(maxColor.G - (randomValue * margin.Z));

             /*       r = (float)(randomValue * (maxColor.R / 2));
                    b = (float)(randomValue * (maxColor.B / 2));
                    g = (float)(randomValue * (maxColor.G / 2));
                    */

                    noisyColors[x + y * resolution] = new Color(r, g, b);

                    // copy to the right
                    noisyColors[(resolution-1 - x) + y * resolution] = new Color(r, g, b);


                    // copy down
                    noisyColors[x + (resolution-1 - y) * resolution] = new Color(r, g, b);

                    // copy down right
                    noisyColors[(resolution-1 - x) + (resolution-1 - y) * resolution] = new Color(r, g, b);

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
