using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Simgame2
{
    public static class WorldGenerator
    {
        /*
        public static void generateBasicMap(WorldMap map, int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

            CreateWaterBorder(map, width, height);
            CreateBeachBorder(map, width, height);

            // create land mass
            float value;
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    //value = (getCell(x - 1, y - 1) + getCell(x - 1, y) + getCell(x, y - 1)) / 3;

                    value = (map.getCell(x - 1, y - 1) + map.getCell(x - 1, y) + map.getCell(x, y - 1));
                    value += map.getCell(x - 2, y - 2) + map.getCell(x - 1, y - 2) + map.getCell(x, y - 2);
                    value += map.getCell(x - 1, y - 2) + map.getCell(x, y - 2);
                    value = value / 8;

                    //           value += (float)(( Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) * 2);
                    //           value = (float)Math.Ceiling(value);
                    if (rand.Next(100) < 10)
                    {
                        value += (float)((Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) * 2);
                        value = (float)Math.Ceiling(value);
                    }
                    if (value < minAltitude)
                    {
                        value = minAltitude;
                    }
                    if (value > maxAltitude)
                    {
                        value = maxAltitude;
                    }

                    map.setCell(x, y, (int)(value));

                }
            }



            // Add lake
            int lakex = 15;
            int lakey = 15;
            int lakeWidth = 6;
            int lakeHeight = 8;


            for (int y = lakey - (lakeWidth / 2); y < lakey + (lakeWidth / 2); y++)
            {
                for (int x = lakex - (lakeHeight / 2); x < lakex + (lakeHeight / 2); x++)
                {
                    map.setCell(x, y, 0);
                }
            }




            // TODO add flat plains, rivers, lakes

        }*/

        /*
        public static void generateRegionMap(WorldMap map,int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

         //   CreateWaterBorder(map, width, height);
        //    CreateBeachBorder(map, width, height);

            
            
            
            float regionFraction = 0.001f; // what fraction of the points are given a random height

            int RegionCenterPointCount = (int)Math.Floor(width * height * regionFraction);

            int[] pointX = new int[RegionCenterPointCount];
            int[] pointY = new int[RegionCenterPointCount];
            int[] pointAltitude = new int[RegionCenterPointCount];
            float[,] CenterPointTexureWeights = new float[RegionCenterPointCount,4];
            
            

            // give those points an altitude and a texture.
            for (int i = 0; i < RegionCenterPointCount; i++)
            {
                pointX[i] = rand.Next(width);
                pointY[i] = rand.Next(height);
                pointAltitude[i] = rand.Next(minAltitude, maxAltitude-1)+1;

                float sum = 0;
                for (int t = 0; t < 4; t++){
                    CenterPointTexureWeights[i, t] = rand.Next(1, 100);
                    sum += CenterPointTexureWeights[i, t];
                }
                for (int t = 0; t < 4; t++)
                {
                    CenterPointTexureWeights[i, t] = CenterPointTexureWeights[i, t] / sum;
                }
            }

            // find for each point on the map, the closest Region Center Point
            int closestRegionPoint = 0;
            int closestTexture = 0;
            double SmallestDistance = double.MaxValue;


            
            double distance;
            for (int y = 0; y < height - 0; y = y+1)
            {
                for (int x = 0; x < width - 0; x = x + 1)
                {
                   


                    // find the closest region center point
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        distance = DistanceSquared(x, y, pointX[r], pointY[r]);

                        

                        if (distance < SmallestDistance)
                        {
                            SmallestDistance = distance;
                            closestRegionPoint = r;
                            
                        }
                    }

                    double pointDistanceToRegionPoint = SmallestDistance;

                    // find region point in line with our point to extrapolate the texture weights
                    SmallestDistance = double.MaxValue;
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        if ((pointX[r] <= pointX[closestRegionPoint] && x <= pointX[closestRegionPoint]) ||
                            (pointX[r] > pointX[closestRegionPoint] && x > pointX[closestRegionPoint]))
                        {
                            if ((pointY[r] <= pointY[closestRegionPoint] && y <= pointY[closestRegionPoint]) ||
                            (pointY[r] > pointY[closestRegionPoint] && y > pointY[closestRegionPoint]))
                            {
                                // point lays in the same quadrant (relative to our regioncenter point) as our current point

                                distance = DistanceSquared(pointX[closestRegionPoint], pointY[closestRegionPoint], pointX[r], pointY[r]);

                                if (distance < SmallestDistance)
                                {
                                    SmallestDistance = distance;
                                    closestTexture = r;

                                }


                            }
                        }
                    }

                    // interpolate between the found points

                    double dif;
                    float[] texuresWeights = new float[4];
                    for (int i = 0 ; i < 4; i++)
                    {
                        dif = CenterPointTexureWeights[closestRegionPoint,i] - CenterPointTexureWeights[closestTexture,i];

                      

                        texuresWeights[i] = (float)(CenterPointTexureWeights[closestRegionPoint, i] + dif / (pointDistanceToRegionPoint));


                    //    dif = pointDistance / (pointTex[closestTexture, i]);
                   //     texuresWeights[i] = (float)(pointTex[closest, i] * dif);




                     //   texuresWeights[i] = pointTex[closestTexture, i] + (pointTex[closest, i] - pointTex[closestTexture, i]);
                     //   texuresWeights[i] *= (float)(pointDistance / SmallestDistance);

                      //  texuresWeights[i] = (float)(pointTex[closest, i]+ ((pointTex[closestTexture, i]-pointTex[closest, i])/SmallestDistance) * pointDistance);

                     //   texuresWeights[i] = lerp((float)pointTex[closest, i], (float)pointTex[closestTexture, i], (float)(pointDistance / SmallestDistance));
                          



                        if (pointDistanceToRegionPoint < 0.00001f)
                        {
                            texuresWeights[i] = CenterPointTexureWeights[closestRegionPoint, i];
                        }



                    }









                    map.setCell(x, y, pointAltitude[closestRegionPoint], texuresWeights[0], texuresWeights[1], texuresWeights[2], texuresWeights[3], closestRegionPoint);
                    


               //     map.setCell(x+1, y, pointAltitude[closest]);
             //       map.setCell(x, y+1, pointAltitude[closest]);
              //      map.setCell(x+1, y+1, pointAltitude[closest]);
                    SmallestDistance = double.MaxValue;
                }
            }







       


            map.pointX = pointX;
            map.pointY = pointY;
            map.pointAltitude = pointAltitude;

            Filter(map, width, height, minAltitude, maxAltitude);


      //      CreateRiver(map, width, height, 8);

      //      CreateMountainBorder(map, width, height);
            

            map.SetResources(GenerateResources(RegionCenterPointCount));




       
        }*/




        /*
        public static void generateRegionMap(WorldMap map,int width, int height, int minAltitude, int maxAltitude)
   {
       Random rand = new Random();

    //   CreateWaterBorder(map, width, height);
   //    CreateBeachBorder(map, width, height);

            
            
            
       float regionFraction = 0.001f; // what fraction of the points are given a random height

       int RegionCenterPointCount = (int)Math.Floor(width * height * regionFraction);

       int[] pointX = new int[RegionCenterPointCount];
       int[] pointY = new int[RegionCenterPointCount];
       int[] pointAltitude = new int[RegionCenterPointCount];
       float[,] CenterPointTexureWeights = new float[RegionCenterPointCount,4];
            
            

       // give those points an altitude and a texture.
       for (int i = 0; i < RegionCenterPointCount; i++)
       {
           pointX[i] = rand.Next(width);
           pointY[i] = rand.Next(height);
           pointAltitude[i] = rand.Next(minAltitude, maxAltitude-1)+1;

           float sum = 0;
           for (int t = 0; t < 4; t++){
               CenterPointTexureWeights[i, t] = rand.Next(1, 100);
               sum += CenterPointTexureWeights[i, t];
           }
           for (int t = 0; t < 4; t++)
           {
               CenterPointTexureWeights[i, t] = CenterPointTexureWeights[i, t] / sum;
           }
       }

       // find for each point on the map, the closest Region Center Point
       int closestRegionPoint = 0;
       int closestTexture = 0;
       double SmallestDistance = double.MaxValue;


            
       double distance;
       for (int y = 0; y < height - 0; y = y+1)
       {
           for (int x = 0; x < width - 0; x = x + 1)
           {
                   


               // find the closest region center point
               for (int r = 0; r < RegionCenterPointCount; r++)
               {
                   distance = DistanceSquared(x, y, pointX[r], pointY[r]);

                        

                   if (distance < SmallestDistance)
                   {
                       SmallestDistance = distance;
                       closestRegionPoint = r;
                            
                   }
               }

               double pointDistanceToRegionPoint = SmallestDistance;

               // find region point in line with our point to extrapolate the texture weights
               SmallestDistance = double.MaxValue;
               for (int r = 0; r < RegionCenterPointCount; r++)
               {
                   if ((pointX[r] <= pointX[closestRegionPoint] && x <= pointX[closestRegionPoint]) ||
                       (pointX[r] > pointX[closestRegionPoint] && x > pointX[closestRegionPoint]))
                   {
                       if ((pointY[r] <= pointY[closestRegionPoint] && y <= pointY[closestRegionPoint]) ||
                       (pointY[r] > pointY[closestRegionPoint] && y > pointY[closestRegionPoint]))
                       {
                           // point lays in the same quadrant (relative to our regioncenter point) as our current point

                           distance = DistanceSquared(pointX[closestRegionPoint], pointY[closestRegionPoint], pointX[r], pointY[r]);

                           if (distance < SmallestDistance)
                           {
                               SmallestDistance = distance;
                               closestTexture = r;

                           }


                       }
                   }
               }

               // interpolate between the found points

               double dif;
               float[] texuresWeights = new float[4];
               for (int i = 0 ; i < 4; i++)
               {
                   dif = CenterPointTexureWeights[closestRegionPoint,i] - CenterPointTexureWeights[closestTexture,i];

                      

                   texuresWeights[i] = (float)(CenterPointTexureWeights[closestRegionPoint, i] + dif / (pointDistanceToRegionPoint));


               //    dif = pointDistance / (pointTex[closestTexture, i]);
              //     texuresWeights[i] = (float)(pointTex[closest, i] * dif);




                //   texuresWeights[i] = pointTex[closestTexture, i] + (pointTex[closest, i] - pointTex[closestTexture, i]);
                //   texuresWeights[i] *= (float)(pointDistance / SmallestDistance);

                 //  texuresWeights[i] = (float)(pointTex[closest, i]+ ((pointTex[closestTexture, i]-pointTex[closest, i])/SmallestDistance) * pointDistance);

                //   texuresWeights[i] = lerp((float)pointTex[closest, i], (float)pointTex[closestTexture, i], (float)(pointDistance / SmallestDistance));
                          



                   if (pointDistanceToRegionPoint < 0.00001f)
                   {
                       texuresWeights[i] = CenterPointTexureWeights[closestRegionPoint, i];
                   }



               }









               map.setCell(x, y, pointAltitude[closestRegionPoint], texuresWeights[0], texuresWeights[1], texuresWeights[2], texuresWeights[3], closestRegionPoint);
                    


          //     map.setCell(x+1, y, pointAltitude[closest]);
        //       map.setCell(x, y+1, pointAltitude[closest]);
         //      map.setCell(x+1, y+1, pointAltitude[closest]);
               SmallestDistance = double.MaxValue;
           }
       }*/


        public static ResourceCell[] GenerateResources(int sectorCount)
        {
            Random rnd = new Random();
            ResourceCell[] resources = new ResourceCell[sectorCount];
            for (int i = 0; i < sectorCount; i++)
            {
                resources[i] = new ResourceCell();
                resources[i].Randomize(rnd);
            }
            return resources;
        }


        static float lerp(float v0, float v1, float t)
        {
            return (1 - t) * v0 + t * v1;
        }



/*
        public static void generateRegionMap2(WorldMap map, int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

            CreateWaterBorder(map, width, height);
            CreateBeachBorder(map, width, height);






            // deteming region center points

            float regionFraction = 0.01f; // what fraction of the points are given a random height

            int RegionCenterPointCount = (int)Math.Floor(width * height * regionFraction);

            int[] pointX = new int[RegionCenterPointCount];
            int[] pointY = new int[RegionCenterPointCount];
            int[] pointAltitude = new int[RegionCenterPointCount];

            // give those points an altitude.
            for (int i = 0; i < RegionCenterPointCount; i++)
            {
                pointX[i] = rand.Next(width);
                pointY[i] = rand.Next(height);
                pointAltitude[i] = rand.Next(minAltitude, maxAltitude);
            }

            List<int>[] boundaries = new List<int>[RegionCenterPointCount];
            for (int i = 0; i < RegionCenterPointCount;i++ )
            {
                boundaries[i] = new List<int>();
            }
            int x, y;
            bool found =false;

            for (int i = 0; i < RegionCenterPointCount; i++)
            {
                x = pointX[i]; 
                y = pointY[i];

                double SmallestDistance = 1;
                double distance;
                for (int c = y+1; c < height; c++)
                {
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        if (r == i) { continue; }
                        distance = DistanceSquared(x, c, pointX[r], pointY[r]);
                        if (distance < SmallestDistance)
                        {
                            boundaries[i].Add(c);
                            found = true;
                            break;
                        }
                    }
                    SmallestDistance++;
                    if (found){ break;}
                }

            }




            // find for each point on the map, the closest Region Center Point
            int closest = 0;
            double SmallestDistance = double.MaxValue;



            double distance;
            for (int y = 2; y < height - 2; y = y + 2)
            {
                for (int x = 2; x < width - 2; x = x + 2)
                {
                    // find the closest region center point
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        distance = DistanceSquared(x, y, pointX[r], pointY[r]);



                        if (distance < SmallestDistance)
                        {
                            SmallestDistance = distance;
                            closest = r;

                        }
                    }

                    map.setCell(x, y, pointAltitude[closest]);
                    map.setCell(x + 1, y, pointAltitude[closest]);
                    map.setCell(x, y + 1, pointAltitude[closest]);
                    map.setCell(x + 1, y + 1, pointAltitude[closest]);
                    SmallestDistance = double.MaxValue;
                }
            }

            map.pointX = pointX;
            map.pointY = pointY;
            map.pointAltitude = pointAltitude;
            

        }
*/

/*
        private static void Filter(WorldMap map, int width, int height, int minAltitude, int maxAltitude)
        {
            float value;
            for (int y = 3; y < height - 3; y = y + 1)
            {
                for (int x = 3; x < width - 3; x = x + 1)
                {



                    value = map.getCell(x - 1, y - 1) + map.getCell(x - 1, y) + map.getCell(x - 1, y + 1);
                    value += map.getCell(x + 1, y - 1) + map.getCell(x + 1, y) + map.getCell(x + 1, y + 1);
                    value += map.getCell(x, y - 1) + map.getCell(x, y + 1) + map.getCell(x, y);
                    value = value / 8;

                    if (value < minAltitude)
                    {
                        value = minAltitude;
                    }
                    if (value > maxAltitude)
                    {
                        value = maxAltitude;
                    }

                    map.setCell(x, y, (int)(value));
                }
            }

        }*/
        
        private static int getCellAdress(int x, int y, int width)
        {
            return (y * width) + x;
        }


        private static double DistanceSquared(int x1, int y1, int x2, int y2)
        {
            return ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
         //   return Math.Pow( (x2 - x1), 2) + Math.Pow( (y2 - y1), 2);
        }

        /*
        private static void CreateBeachBorder(WorldMap map, int width, int height)
        {
            // create beach border
            for (int x = 1; x < width - 1; x++)
            {
                map.setCell(x, 1, 1, 100, 0, 0, 0);
                map.setCell(x, height - 2, 1, 100, 0, 0, 0);

            }
            for (int y = 1; y < height - 1; y++)
            {
                map.setCell(1, y, 1, 100, 0, 0, 0);
                map.setCell(width - 2, y, 1, 100, 0, 0, 0);
            }
        }*/
/*
        private static void CreateWaterBorder(WorldMap map, int width, int height)
        {
            // create water border
            for (int x = 0; x < width; x++)
            {
                map.setCell(x, 0, 0, 100, 0, 0, 0);
                map.setCell(x, height - 1, 0, 100, 0, 0, 0);

            }
            for (int y = 0; y < height; y++)
            {
                map.setCell(0, y, 0, 100, 0, 0, 0);
                map.setCell(width - 1, y, 0, 100, 0, 0, 0);
            }
        }
        */
/*
        private static void CreateMountainBorder(WorldMap map, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                map.setCell(x, 0, 50, 100, 0, 0, 0);
                map.setCell(x, height - 1, 50, 100, 0, 0, 0);

            }
            for (int y = 0; y < height; y++)
            {
                map.setCell(0, y, 50, 100, 0, 0, 0);
                map.setCell(width - 1, y, 50, 100, 0, 0, 0);
            }


            for (int x = 1; x < width-1; x++)
            {
                map.setCell(x, 0, 25, 100, 0, 0, 0);
                map.setCell(x, height - 1, 25, 100, 0, 0, 0);

            }
            for (int y = 1; y < height-1; y++)
            {
                map.setCell(0, y, 25, 100, 0, 0, 0);
                map.setCell(width - 1, y, 25, 100, 0, 0, 0);
            }

        }
        */


/*

        private static void CreateRiver(WorldMap map, int width, int height, int riverWidth){

            Random rnd = new Random();

            int[] riverRowPoints = new int[height];
            riverRowPoints[0] = width / 2;
            for (int i = 1; i < height; i++)
            {
                int offset = rnd.Next(2) - 1;

                riverRowPoints[i] = riverRowPoints[i - 1] + offset;

                for (int j = riverRowPoints[i] - (riverWidth / 2); j < riverRowPoints[i] + (riverWidth / 2); j++)
                {
                    map.setCell(j, i, 0);
                }
            }



        }

        */



        public static void generateRegionMapLOD(out VertexMultitextured[] vertices, out int[] sectors, out ResourceCell[] resources, int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

            int _vertexCount = width * height;

            //Initialize our array to hold the vertices
            vertices = new VertexMultitextured[_vertexCount];
            sectors = new int[_vertexCount];

            float mapCellScaleDivTextureSize = (float)LODTerrain.LODTerrain.mapCellScale / (float)LODTerrain.LODTerrain.textureSize;


            //   CreateWaterBorder(map, width, height);
            //    CreateBeachBorder(map, width, height);




            float regionFraction = 0.0005f;  //0.001f; // what fraction of the points are given a random height

            int RegionCenterPointCount = (int)Math.Floor(width * height * regionFraction);

            int[] pointX = new int[RegionCenterPointCount];
            int[] pointY = new int[RegionCenterPointCount];
            int[] pointAltitude = new int[RegionCenterPointCount];
            float[,] CenterPointTexureWeights = new float[RegionCenterPointCount, 4];



            // give those points an altitude and a texture.
            for (int i = 0; i < RegionCenterPointCount; i++)
            {
                pointX[i] = rand.Next(width);
                pointY[i] = rand.Next(height);
                pointAltitude[i] = rand.Next(minAltitude, maxAltitude - 1) + 1;

                float sum = 0;
                for (int t = 0; t < 4; t++)
                {
                    CenterPointTexureWeights[i, t] = rand.Next(1, 100);
                    sum += CenterPointTexureWeights[i, t];
                }
                for (int t = 0; t < 4; t++)
                {
                    CenterPointTexureWeights[i, t] = CenterPointTexureWeights[i, t] / sum;
                }
            }




            // find for each point on the map, the closest Region Center Point
            int closestRegionPoint = 0;
            int closestTexture = 0;
            double SmallestDistance = double.MaxValue;



            double distance;
            for (int y = 0; y < height - 0; y = y + 1)
            {
                for (int x = 0; x < width - 0; x = x + 1)
                {



                    // find the closest region center point
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        distance = DistanceSquared(x, y, pointX[r], pointY[r]);



                        if (distance < SmallestDistance)
                        {
                            SmallestDistance = distance;
                            closestRegionPoint = r;

                        }
                    }

                    double pointDistanceToRegionPoint = SmallestDistance;

                    // find region point in line with our point to extrapolate the texture weights
                    SmallestDistance = double.MaxValue;
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        if ((pointX[r] <= pointX[closestRegionPoint] && x <= pointX[closestRegionPoint]) ||
                            (pointX[r] > pointX[closestRegionPoint] && x > pointX[closestRegionPoint]))
                        {
                            if ((pointY[r] <= pointY[closestRegionPoint] && y <= pointY[closestRegionPoint]) ||
                            (pointY[r] > pointY[closestRegionPoint] && y > pointY[closestRegionPoint]))
                            {
                                // point lays in the same quadrant (relative to our regioncenter point) as our current point

                                distance = DistanceSquared(pointX[closestRegionPoint], pointY[closestRegionPoint], pointX[r], pointY[r]);

                                if (distance < SmallestDistance)
                                {
                                    SmallestDistance = distance;
                                    closestTexture = r;

                                }


                            }
                        }
                    }



                    // interpolate between the found points

                    double dif;
                    float[] texuresWeights = new float[4];
                    for (int i = 0; i < 4; i++)
                    {
                        dif = CenterPointTexureWeights[closestRegionPoint, i] - CenterPointTexureWeights[closestTexture, i];



                        texuresWeights[i] = (float)(CenterPointTexureWeights[closestRegionPoint, i] + dif / (pointDistanceToRegionPoint));


                        if (pointDistanceToRegionPoint < 0.00001f)
                        {
                            texuresWeights[i] = CenterPointTexureWeights[closestRegionPoint, i];
                        }



                    }




                    
                    

                    int adress = getCellAdress(x, y, width);

                    vertices[adress] = new VertexMultitextured();
                    vertices[adress].Position = new Vector3(x * LODTerrain.LODTerrain.mapCellScale, pointAltitude[closestRegionPoint] * LODTerrain.LODTerrain.mapHeightScale, y * LODTerrain.LODTerrain.mapCellScale);


                    vertices[adress].TextureCoordinate.X = ((float)(x * mapCellScaleDivTextureSize));
                    vertices[adress].TextureCoordinate.Y = ((float)(y * mapCellScaleDivTextureSize));


                    vertices[adress].TexWeights = new Vector4(texuresWeights[0], texuresWeights[1], texuresWeights[2], texuresWeights[3]);

                    vertices[adress].Normal = new Vector3(0, 0, 0);

                    sectors[adress] = closestRegionPoint;

                    SmallestDistance = double.MaxValue;
                }
            }




//            Filter(vertices, width, height, minAltitude, maxAltitude);



            resources = GenerateResources(RegionCenterPointCount);


        }


        private static void Filter(VertexMultitextured[] vertices, int width, int height, int minAltitude, int maxAltitude)
        {
            float value;
            for (int y = 3; y < height - 3; y = y + 1)
            {
                for (int x = 3; x < width - 3; x = x + 1)
                {
                    


                    value = vertices[getCellAdress(x-1,y-1,width)].Position.Y + 
                        vertices[getCellAdress(x-1,y,width)].Position.Y + 
                        vertices[getCellAdress(x-1,y+1,width)].Position.Y +
                        vertices[getCellAdress(x+1,y-1,width)].Position.Y + 
                        vertices[getCellAdress(x+1,y,width)].Position.Y + 
                        vertices[getCellAdress(x+1,y+1,width)].Position.Y +
                        vertices[getCellAdress(x,y-1,width)].Position.Y + 
                        vertices[getCellAdress(x,y+1,width)].Position.Y + 
                        vertices[getCellAdress(x,y,width)].Position.Y;
                    value = value / 8;

                    if (value < minAltitude)
                    {
                        value = minAltitude;
                    }
                    if (value > maxAltitude)
                    {
                        value = maxAltitude;
                    }
                    Vector3 pos = vertices[getCellAdress(x,y,width)].Position;
                    vertices[getCellAdress(x,y,width)].Position = new Vector3(pos.X, value, pos.Z);
                    
                }
            }

        }




        public static void generateRegionMapLOD2(out VertexMultitextured[] vertices, out int[] sectors, out ResourceCell[] resources, int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

            int _vertexCount = width * height;

            //Initialize our array to hold the vertices
            vertices = new VertexMultitextured[_vertexCount];
            sectors = new int[_vertexCount];




            //   CreateWaterBorder(map, width, height);
            //    CreateBeachBorder(map, width, height);




            float regionFraction = 0.0005f;  //0.001f; // what fraction of the points are given a random height

            int RegionCenterPointCount = (int)Math.Floor(width * height * regionFraction);

            int[] pointX = new int[RegionCenterPointCount];
            int[] pointY = new int[RegionCenterPointCount];
            int[] pointAltitude = new int[RegionCenterPointCount];
            float[,] CenterPointTexureWeights = new float[RegionCenterPointCount, 4];



            // give those points an altitude and a texture.
            for (int i = 0; i < RegionCenterPointCount; i++)
            {
                pointX[i] = rand.Next(width);
                pointY[i] = rand.Next(height);
                pointAltitude[i] = rand.Next(minAltitude, maxAltitude - 1) + 1;

                float sum = 0;
                for (int t = 0; t < 4; t++)
                {
                    CenterPointTexureWeights[i, t] = rand.Next(1, 100);
                    sum += CenterPointTexureWeights[i, t];
                }
                for (int t = 0; t < 4; t++)
                {
                    CenterPointTexureWeights[i, t] = CenterPointTexureWeights[i, t] / sum;
                }
            }




            // find for each point on the map, the closest Region Center Point
            int closestRegionPoint = 0;
            int closestTexture = 0;
            double SmallestDistance = double.MaxValue;



            double distance;
            for (int y = 0; y < height - 0; y = y + 1)
            {
                for (int x = 0; x < width - 0; x = x + 1)
                {



                    // find the closest region center point
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        distance = DistanceSquared(x, y, pointX[r], pointY[r]);



                        if (distance < SmallestDistance)
                        {
                            SmallestDistance = distance;
                            closestRegionPoint = r;

                        }
                    }

                    double pointDistanceToRegionPoint = SmallestDistance;

                    // find region point in line with our point to extrapolate the texture weights
                    SmallestDistance = double.MaxValue;
                    for (int r = 0; r < RegionCenterPointCount; r++)
                    {
                        if ((pointX[r] <= pointX[closestRegionPoint] && x <= pointX[closestRegionPoint]) ||
                            (pointX[r] > pointX[closestRegionPoint] && x > pointX[closestRegionPoint]))
                        {
                            if ((pointY[r] <= pointY[closestRegionPoint] && y <= pointY[closestRegionPoint]) ||
                            (pointY[r] > pointY[closestRegionPoint] && y > pointY[closestRegionPoint]))
                            {
                                // point lays in the same quadrant (relative to our regioncenter point) as our current point

                                distance = DistanceSquared(pointX[closestRegionPoint], pointY[closestRegionPoint], pointX[r], pointY[r]);

                                if (distance < SmallestDistance)
                                {
                                    SmallestDistance = distance;
                                    closestTexture = r;

                                }


                            }
                        }
                    }



                    // interpolate between the found points

                    double dif;
                    float[] texuresWeights = new float[4];
                    for (int i = 0; i < 4; i++)
                    {
                        dif = CenterPointTexureWeights[closestRegionPoint, i] - CenterPointTexureWeights[closestTexture, i];



                        texuresWeights[i] = (float)(CenterPointTexureWeights[closestRegionPoint, i] + dif / (pointDistanceToRegionPoint));


                        if (pointDistanceToRegionPoint < 0.00001f)
                        {
                            texuresWeights[i] = CenterPointTexureWeights[closestRegionPoint, i];
                        }



                    }




                    float mapCellScaleDivTextureSize = (float)LODTerrain.LODTerrain.mapCellScale / (float)LODTerrain.LODTerrain.textureSize;


                    int adress = getCellAdress(x, y, width);

                    vertices[adress] = new VertexMultitextured();
                    vertices[adress].Position = new Vector3(x * LODTerrain.LODTerrain.mapCellScale, pointAltitude[closestRegionPoint] * LODTerrain.LODTerrain.mapHeightScale, y * LODTerrain.LODTerrain.mapCellScale);


                    vertices[adress].TextureCoordinate.X = ((float)(x * mapCellScaleDivTextureSize));
                    vertices[adress].TextureCoordinate.Y = ((float)(y * mapCellScaleDivTextureSize));


                    vertices[adress].TexWeights = new Vector4(texuresWeights[0], texuresWeights[1], texuresWeights[2], texuresWeights[3]);

                    vertices[adress].Normal = new Vector3(0, 0, 0);

                    sectors[adress] = closestRegionPoint;

                    SmallestDistance = double.MaxValue;
                }
            }




            //            Filter(vertices, width, height, minAltitude, maxAltitude);



            resources = GenerateResources(RegionCenterPointCount);


        }






/*
        public static void generateBasicMap(out VertexMultitextured[] vertices, int[] sectors, ResourceCell[] resources, int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

            int _vertexCount = width * height;

            //Initialize our array to hold the vertices
            vertices = new VertexMultitextured[_vertexCount];
            sectors = new int[_vertexCount];

            // create land mass
            float value;
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    //value = (getCell(x - 1, y - 1) + getCell(x - 1, y) + getCell(x, y - 1)) / 3;

                    value = (map.getCell(x - 1, y - 1) + map.getCell(x - 1, y) + map.getCell(x, y - 1));
                    value += map.getCell(x - 2, y - 2) + map.getCell(x - 1, y - 2) + map.getCell(x, y - 2);
                    value += map.getCell(x - 1, y - 2) + map.getCell(x, y - 2);
                    value = value / 8;

                    //           value += (float)(( Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) * 2);
                    //           value = (float)Math.Ceiling(value);
                    if (rand.Next(100) < 10)
                    {
                        value += (float)((Math.Sin((x + SimplexNoise.Noise.Generate(x * 5, y * 5) / 2) * 50)) * 2);
                        value = (float)Math.Ceiling(value);
                    }
                    if (value < minAltitude)
                    {
                        value = minAltitude;
                    }
                    if (value > maxAltitude)
                    {
                        value = maxAltitude;
                    }

                    map.setCell(x, y, (int)(value));

                }
            }



            // Add lake
            int lakex = 15;
            int lakey = 15;
            int lakeWidth = 6;
            int lakeHeight = 8;


            for (int y = lakey - (lakeWidth / 2); y < lakey + (lakeWidth / 2); y++)
            {
                for (int x = lakex - (lakeHeight / 2); x < lakex + (lakeHeight / 2); x++)
                {
                    map.setCell(x, y, 0);
                }
            }




            // TODO add flat plains, rivers, lakes

        }*/


    }
}
