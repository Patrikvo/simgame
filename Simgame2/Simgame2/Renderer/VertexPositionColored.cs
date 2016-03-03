using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.Renderer
{
    public struct VertexPositionColored : IVertexType
    {
        public Vector3 Position;
        public Color Color1;

        public VertexPositionColored(Vector3 position, Color color1)
        {
            this.Position = position;
            this.Color1 = color1;
        }

        public static int SizeInBytes = 7 * 4;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );


        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

    }
}
