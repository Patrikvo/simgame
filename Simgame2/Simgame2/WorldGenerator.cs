using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2
{
    public static class WorldGenerator
    {
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

        }


        public static void generateRegionMap(WorldMap map, int width, int height, int minAltitude, int maxAltitude)
        {
            Random rand = new Random();

            CreateWaterBorder(map, width, height);
            CreateBeachBorder(map, width, height);

            
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

            // find for each point on the map, the closest Region Center Point
            int closest = 0;
            double SmallestDistance = double.MaxValue;


            
            double distance;
            for (int y = 2; y < height - 2; y = y+2)
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
                    map.setCell(x+1, y, pointAltitude[closest]);
                    map.setCell(x, y+1, pointAltitude[closest]);
                    map.setCell(x+1, y+1, pointAltitude[closest]);
                    SmallestDistance = double.MaxValue;
                }
            }
            




          





/*
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
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
                    SmallestDistance = double.MaxValue;
                }
            }
            */


        }


        

        private static double DistanceSquared(int x1, int y1, int x2, int y2)
        {
            return Math.Pow( (x2 - x1), 2) + Math.Pow( (y2 - y1), 2);
        }


        private static void CreateBeachBorder(WorldMap map, int width, int height)
        {
            // create beach border
            for (int x = 1; x < width - 1; x++)
            {
                map.setCell(x, 1, 1);
                map.setCell(x, height - 2, 1);

            }
            for (int y = 1; y < height - 1; y++)
            {
                map.setCell(1, y, 1);
                map.setCell(width - 2, y, 1);
            }
        }

        private static void CreateWaterBorder(WorldMap map, int width, int height)
        {
            // create water border
            for (int x = 0; x < width; x++)
            {
                map.setCell(x, 0, 0);
                map.setCell(x, height - 1, 0);

            }
            for (int y = 0; y < height; y++)
            {
                map.setCell(0, y, 0);
                map.setCell(width - 1, y, 0);
            }
        }


    }
}
