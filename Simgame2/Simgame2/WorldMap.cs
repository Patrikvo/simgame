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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class WorldMap : Microsoft.Xna.Framework.GameComponent
    {
        public WorldMap(Game game, int mapWidth, int mapHeight)
            : base(game)
        {
            this.width = mapWidth;
            this.height = mapHeight;

            this.heightMap = new int[width * height];
            generateMap();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }



        public int getCellAdress(int x, int y)
        {
            return (y * this.width) + x;
        }

        public int getCell(int x, int y)
        {
            if (x < 0 || y < 0 || x > this.width || y > this.height)
            {
                return 0;
            }

            return this.heightMap[getCellAdress(x, y)];
        }

        public void setCell(int x, int y, int value)
        {
            this.heightMap[getCellAdress(x, y)] = value;
        }


        public void lowerCellHeight(int x, int y, ushort amount)
        {
            // TODO implement
        }

        public void raiseCellHeight(int x, int y, ushort amount)
        {
            // TODO implement
        }

        public void generateMap()
        {
            Random rand = new Random();
            // TODO implement better map generation

            

            // create water border
            for (int x = 0; x < this.width; x++)
            {
                setCell(x, 0, 0);
                setCell(x, height - 1, 0);

            }
            for (int y = 0; y < this.height; y++)
            {
                setCell(0, y, 0);
                setCell(this.width - 1, y, 0);
            }

            // create beach border
            for (int x = 1; x < this.width-1; x++)
            {
                setCell(x, 1, 1);
                setCell(x, height - 2, 1);

            }
            for (int y = 1; y < this.height-1; y++)
            {
                setCell(1, y, 1);
                setCell(this.width - 2, y, 1);
            }

            int modifier;
            // create land mass
            for (int y = 2; y < this.height-2; y++)
            {
                modifier = 1;
                for (int x = 2; x < this.width-2; x++)
                {
                    //int randval = rand.Next(0, 100);

                    modifier += rand.Next(-2, 2);
                    if (modifier < 1) modifier = 1;
                    if (modifier > 30) modifier = 30;

                    setCell(x, y, modifier);
                }
            }


            SetUpVertices();
            SetUpIndices();

            vertices = CalculateNormals(vertices, indices);

        }

        public void GenerateView()
        {
            //TODO write setupvertices and setupindices so that these contain only those vertices that are withing the frustrum


//            frustum.Contains(Vector3 point)

       //     List<VertexPositionNormalColored> verticesList = new List<VertexPositionNormalColored>();
            int regionWidth = 0;
            int regionHeight = 0;

            int regionLeft = -1;
            int regionRight = -1;
            int regionUp = -1;
            int regionDown = -1;

            // look for upper left corner of the visual region
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    // four point of the square cell
                    Vector3 v1 = new Vector3(x, getCell(x, y), -y);
                    Vector3 v2 = new Vector3(x+1, getCell(x, y), -y);
                    Vector3 v3 = new Vector3(x, getCell(x, y), -(y+1));
                    Vector3 v4 = new Vector3(x+1, getCell(x, y), -(y+1));
                    if (frustum.Contains(v1) == ContainmentType.Contains ||
                        frustum.Contains(v2) == ContainmentType.Contains ||
                        frustum.Contains(v3) == ContainmentType.Contains ||
                        frustum.Contains(v4) == ContainmentType.Contains)
                    {
                        regionLeft = x;
                        regionUp = y;
                        break;
                    }
                }
                if (regionLeft != -1)
                {
                    break;
                }
            }


            // look for the upper right corner of the visual region
            for (int x = regionLeft; x < this.width; x++)
            {
                for (int y = regionUp; y < this.height; y++)
                {
                    // four point of the square cell
                    Vector3 v1 = new Vector3(x, getCell(x, y), -y);
                    Vector3 v2 = new Vector3(x + 1, getCell(x, y), -y);
                    Vector3 v3 = new Vector3(x, getCell(x, y), -(y + 1));
                    Vector3 v4 = new Vector3(x + 1, getCell(x, y), -(y + 1));
                    if (frustum.Contains(v1) == ContainmentType.Contains ||
                        frustum.Contains(v2) == ContainmentType.Contains ||
                        frustum.Contains(v3) == ContainmentType.Contains ||
                        frustum.Contains(v4) == ContainmentType.Contains)
                    {
                       
                    }
                    else
                    {
                        regionRight = x;

                        // region reaches till right edge
                        if (regionRight == 0)
                        {
                            regionRight = this.width;
                        }

                        break;
                    }


                }
                if (regionRight != -1)
                {
                    break;
                }
            }

            // look for the lower left corner of the visual region
            for (int y = regionUp; y < this.height; y++)
            {

                // four point of the square cell
                Vector3 v1 = new Vector3(regionLeft, getCell(regionLeft, y), -y);
                Vector3 v2 = new Vector3(regionLeft + 1, getCell(regionLeft, y), -y);
                Vector3 v3 = new Vector3(regionLeft, getCell(regionLeft, y), -(y + 1));
                Vector3 v4 = new Vector3(regionLeft + 1, getCell(regionLeft, y), -(y + 1));
                if (frustum.Contains(v1) == ContainmentType.Contains ||
                        frustum.Contains(v2) == ContainmentType.Contains ||
                        frustum.Contains(v3) == ContainmentType.Contains ||
                        frustum.Contains(v4) == ContainmentType.Contains)
                { }
                else
                {
                    regionDown = y;
                    break;
                }
            }

            regionWidth = regionRight - regionLeft;
            regionHeight = regionDown - regionUp;

            vertices = new VertexPositionNormalColored[regionWidth * regionHeight];
            for (int x = regionLeft; x < regionRight; x++)
            {
                for (int y = regionUp; y < regionDown; y++)
                {
                    int adress = (x - regionLeft) + (y - regionUp) * this.width;
                    vertices[adress].Position = new Vector3(x, getCell(x, y), -y);

                    if (getCell(x, y) == 0)
                        vertices[adress].Color = Color.Blue;
                    else if (getCell(x, y) < 10)
                        vertices[adress].Color = Color.Green;
                    else if (getCell(x, y) < 25)
                        vertices[adress].Color = Color.Brown;
                    else
                        vertices[adress].Color = Color.White;

                }
            }




            indices = new Int16[(regionWidth - 1) * (regionHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < regionHeight - 1; y++)
            {
                for (int x = 0; x < regionRight - 1; x++)
                {
                    Int16 lowerLeft = (Int16)(x + y * this.width);
                    Int16 lowerRight = (Int16)((x + 1) + y * this.width);
                    Int16 topLeft = (Int16)(x + (y + 1) * this.width);
                    Int16 topRight = (Int16)((x + 1) + (y + 1) * this.width);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }


            vertices = CalculateNormals(vertices, indices);

        }

        

        private void SetUpVertices()
        {
            vertices = new VertexPositionNormalColored[this.width * this.height];
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    vertices[x + y * this.width].Position = new Vector3(x, getCell(x, y), -y);

                    if (getCell(x,y) == 0)
                        vertices[x + y * this.width].Color = Color.Blue;
                    else if (getCell(x, y) < 10)
                        vertices[x + y * this.width].Color = Color.Green;
                    else if (getCell(x, y) < 25)
                        vertices[x + y * this.width].Color = Color.Brown;
                    else
                        vertices[x + y * this.width].Color = Color.White;

                }
            }
        }

        
        private void SetUpIndices()
        {
            indices = new Int16[(this.width - 1) * (this.height - 1) * 6];
            int counter = 0;
            for (ushort y = 0; y < this.height - 1; y++)
            {
                for (ushort x = 0; x < this.width - 1; x++)
                {
                    Int16 lowerLeft = (Int16)(x + y * this.width);
                    Int16 lowerRight = (Int16)((x + 1) + y * this.width);
                    Int16 topLeft = (Int16)(x + (y + 1) * this.width);
                    Int16 topRight = (Int16)((x + 1) + (y + 1) * this.width);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }


        private VertexPositionNormalColored[] CalculateNormals(VertexPositionNormalColored[] vertices, Int16[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }


        public int width { get; set; }
        public int height { get; set; }

        private int[] heightMap;

        public VertexPositionNormalColored[] vertices;
        public Int16[] indices;

        public const int WaterLevel = 0;

        public BoundingFrustum frustum;

    }
}
