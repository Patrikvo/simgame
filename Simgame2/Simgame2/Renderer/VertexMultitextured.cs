using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.Renderer
{
    public struct VertexMultitextured : IVertexType
    {
        public VertexMultitextured(
         Vector3 position,
         Vector3 normal,
         Vector4 textureCoordinate,
         Vector4 texWeights,
         Vector3 Tangent,
         Vector3 Bitangent
            )
        {
            this.Position = position;
            this.Normal = normal;
            this.TextureCoordinate = textureCoordinate;
            this.TexWeights = texWeights;
            this.Tangent = Tangent;
            this.BiTangent = Bitangent;
        }

        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;
        public Vector3 Tangent;
        public Vector3 BiTangent; // : BINORMAL0;

        public static int SizeInBytes = (3 + 3 + 4 + 4 + 3) * sizeof(float);
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
     (
         new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
         new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
         new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
         new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
         new VertexElement(sizeof(float) * 14, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
         new VertexElement(sizeof(float) * 17, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
     );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
}
