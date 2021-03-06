﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simgame2.Renderer;

// source:   http://www.dustinhorne.com/post/2011/08/28/XNA-Terrain-with-LOD-Part-3-Structuring-the-QuadTree


namespace Simgame2.LODTerrain
{
    public class TreeVertexCollection
    {
        // VertexMultitextured


        public VertexMultitextured[] Vertices;
    //    Vector3 _position;
        int _topSize;
        int _halfSize;
        int _vertexCount;
        Vector4 MapSize;
     //   int _scale;

        public int[] sector;
        public ResourceCell[] resources;

        public VertexMultitextured this[int index]
        {
            get { return Vertices[index]; }
            set { Vertices[index] = value; }
        }


        public void Store(GameSession.GameStorage storage)
        {
            storage.Write(MapSize);     //  Vector4 MapSize;

            // VertexMultitextured[] Vertices;
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                storage.Write(this.Vertices[i]);
            }

            // int[] sector;
            storage.Write(sector.Length);
            for (int i = 0; i < sector.Length; i++)
            {
                storage.Write(sector[i]);
            }
            
            // ResourceCell[] resources;
            storage.Write(resources.Length);
            for (int i = 0; i < this.resources.Length; i++)
            {
                resources[i].Store(storage);
            }

        }



        public TreeVertexCollection(Vector4 mapSize)
        {
            this.MapSize = mapSize;
            _topSize = (int)mapSize.X - 1;
            _halfSize = _topSize / 2;
            _vertexCount = (int)(mapSize.X * mapSize.Y);

            //Initialize our array to hold the vertices
            Vertices = new VertexMultitextured[_vertexCount];

            //Our method to populate the vertex collection
            BuildVertices(mapSize);

            //Our method to  calculate the normals for all vertices
            CalculateAllNormals();

            

        }

        public TreeVertexCollection(GameSession.GameStorage storage)
        {
            this.MapSize = storage.ReadVector4();
            _topSize = (int)this.MapSize.X - 1;
            _halfSize = _topSize / 2;
            _vertexCount = (int)(this.MapSize.X * this.MapSize.Y);

            int vertexCount = storage.ReadInt();

            //Initialize our array to hold the vertices
            Vertices = new VertexMultitextured[_vertexCount];

            //Our method to populate the vertex collection
            // VertexMultitextured[] Vertices;
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                this.Vertices[i] = storage.ReadVertexMultiTextured();
            }

            // int[] sector;
            sector = new int[storage.ReadInt()];
            for (int i = 0; i < sector.Length; i++)
            {
                sector[i] = storage.ReadInt();
            }

            // ResourceCell[] resources;
            resources = new ResourceCell[storage.ReadInt()];
            for (int i = 0; i < this.resources.Length; i++)
            {
                resources[i] = new ResourceCell();
                resources[i].Restore(storage);
            }

            //Our method to  calculate the normals for all vertices
            CalculateAllNormals();



        }

        private void BuildVertices(Vector4 mapSize)
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


            sw.Start();
           WorldGenerator.generateRegionMapLOD(out this.Vertices, out this.sector, out this.resources, (int)mapSize.X, (int)mapSize.Y, (int)mapSize.Z, (int)mapSize.W);
           sw.Stop();
           Console.WriteLine("map generation took: " + sw.ElapsedMilliseconds.ToString() + " ms");
           GameSession.GameStorage storage = new GameSession.GameStorage("map.dat", true);
           storage.Write(mapSize);
           storage.Write(this.Vertices.Length);
           for (int i = 0; i < this.Vertices.Length; i++)
           {
               storage.Write(this.Vertices[i]);
           }
           storage.Write(this.sector.Length);
           for (int i = 0; i < this.sector.Length; i++)
           {
               storage.Write(this.sector[i]);
           }
           storage.Write(this.resources.Length);
           for (int i = 0; i < this.resources.Length; i++)
           {
               this.resources[i].Store(storage);
           }

           storage.Close();
        }

        private void CalculateAllNormals()
        {
            if (_vertexCount < 9)
                return;

            int i = _topSize + 2, j = 0, k = i + _topSize;

            for (int n = 0; i <= (_vertexCount - _topSize) - 2; i += 2, n++, j += 2, k += 2)
            {

                if (n == _halfSize)
                {
                    n = 0;
                    i += _topSize + 2;
                    j += _topSize + 2;
                    k += _topSize + 2;
                }

                //Calculate normals for each of the 8 triangles
                SetNormals(i, j, j + 1);
                SetNormals(i, j + 1, j + 2);
                SetNormals(i, j + 2, i + 1);
                SetNormals(i, i + 1, k + 2);
                SetNormals(i, k + 2, k + 1);
                SetNormals(i, k + 1, k);
                SetNormals(i, k, i - 1);
                SetNormals(i, i - 1, j);
            }
        }

        private void SetNormals(int idx1, int idx2, int idx3)
        {
            if (idx3 >= Vertices.Length)
                idx3 = Vertices.Length - 1;

            var normal = Vector3.Cross(Vertices[idx2].Position - Vertices[idx1].Position, Vertices[idx1].Position - Vertices[idx3].Position);
            normal.Normalize();
            Vertices[idx1].Normal += normal;
            Vertices[idx2].Normal += normal;
            Vertices[idx3].Normal += normal;


            Vector3 v1 = Vertices[idx1].Position;
            Vector3 v2 = Vertices[idx2].Position;
            Vector3 v3 = Vertices[idx3].Position;

            // These are the texture coordinate of the triangle  
            Vector4 w1 = Vertices[idx1].TextureCoordinate; 
            Vector4 w2 = Vertices[idx2].TextureCoordinate; 
            Vector4 w3 = Vertices[idx3].TextureCoordinate;

            float x1 = v2.X - v1.X;
            float x2 = v3.X - v1.X;
            float y1 = v2.Y - v1.Y;
            float y2 = v3.Y - v1.Y;
            float z1 = v2.Z - v1.Z;
            float z2 = v3.Z - v1.Z;

            float s1 = w2.X - w1.X;
            float s2 = w3.X - w1.X;
            float t1 = w2.Y - w1.Y;
            float t2 = w3.Y - w1.Y;

            float r = 1.0f / (s1 * t2 - s2 * t1);
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            // Gram-Schmidt orthogonalize  
            Vector3 tangent = sdir - normal * Vector3.Dot(normal, sdir);
            tangent.Normalize();

            // Calculate handedness (here maybe you need to switch >= with <= depend on the geometry winding order)  
            float tangentdir = (Vector3.Dot(Vector3.Cross(normal, sdir), tdir) >= 0.0f) ? 1.0f : -1.0f;
            Vector3 binormal = Vector3.Cross(normal, tangent) * tangentdir;

            Vertices[idx1].Tangent = tangent;
            Vertices[idx2].Tangent = tangent;
            Vertices[idx3].Tangent = tangent;

            Vertices[idx1].BiTangent = binormal;
            Vertices[idx2].BiTangent = binormal;
            Vertices[idx2].BiTangent = binormal;


        }




    }
}
