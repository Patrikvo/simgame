using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Simgame2.GameSession
{
    public class GameStorage
    {

        public GameStorage(string filename, bool IsWriter)
        {
            this.IsWriter = IsWriter;

            try
            {
                if (IsWriter){
                    this.writer = new BinaryWriter(File.Open(filename, FileMode.Create));
                }
                else
                {
                    if (File.Exists(filename))
                    {
                        reader = new BinaryReader(File.Open(filename, FileMode.Open));
                    }
                    else
                    {
                        throw new FileNotFoundException("File " + filename + " Not found");
                    }
                }
            }
            catch (Exception ex)
            {
                this.LastException = ex;
                Console.WriteLine("An exception occured: " + ex.ToString());
            }
        }


        public void flush()
        {
            if (IsWriter) { this.writer.Flush(); }
        }


        public void Close()
        {
            this.flush();
            if (IsWriter)
            {
                this.writer.Close();
                this.writer.Dispose();
            }
            else
            {
                this.reader.Close();
                this.reader.Dispose();
            }
        }

        public void Write(Int32 value)
        {
            Debug.Assert(IsWriter == true, "trying to write to a reader");

            writer.Write(value);
        }

        public void Write(float value)
        {
            Debug.Assert(IsWriter == true, "trying to write to a reader");

            writer.Write(value);
        }

        public void Write(Vector3 vector)
        {
            this.Write(vector.X); this.Write(vector.Y); this.Write(vector.Z);
        }

        public void Write(Vector4 vector)
        {
            this.Write(vector.X); this.Write(vector.Y); this.Write(vector.Z); this.Write(vector.W);
        }

        public void Write(Simgame2.Renderer.VertexMultitextured vertex)
        {
            this.Write(vertex.BiTangent); this.Write(vertex.Normal); this.Write(vertex.Position); this.Write(vertex.Tangent); this.Write(vertex.TextureCoordinate);
            this.Write(vertex.TexWeights);
        }

        // VertexMultitextured

        public void Write(Matrix matrix)
        {
            this.Write(matrix.M11); this.Write(matrix.M12); this.Write(matrix.M13); this.Write(matrix.M14);
            this.Write(matrix.M21); this.Write(matrix.M22); this.Write(matrix.M23); this.Write(matrix.M24);
            this.Write(matrix.M31); this.Write(matrix.M32); this.Write(matrix.M33); this.Write(matrix.M34);
            this.Write(matrix.M41); this.Write(matrix.M42); this.Write(matrix.M43); this.Write(matrix.M44);
        }


        public int ReadInt()
        {
            Debug.Assert(IsWriter == false, "trying to read from a writer");

            return reader.ReadInt32();
        }



        public float ReadSingle()
        {
            Debug.Assert(IsWriter == false, "trying to read from a writer");

            return reader.ReadSingle();
        }




        public Vector3 ReadVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Matrix ReadMatrix()
        {
            return new Matrix(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle(),
                                ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle(),
                                ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle(),
                                ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Simgame2.Renderer.VertexMultitextured ReadVertexMultiTextured()
        {
            Simgame2.Renderer.VertexMultitextured vertex;
            vertex.BiTangent = this.ReadVector3();
            vertex.Normal = this.ReadVector3();
            vertex.Position = this.ReadVector3();
            vertex.Tangent = this.ReadVector3();
            vertex.TextureCoordinate = this.ReadVector4();
            vertex.TexWeights = this.ReadVector4();

            return vertex;
        }


        public bool IsWriter { get; private set; }
        public Exception LastException { get; set; }



        private BinaryWriter writer;
        private BinaryReader reader;

    }
}
