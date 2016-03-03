using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simgame2.Renderer;

namespace Simgame2.LODTerrain
{
    internal class BufferManager
    {
        int _active = 0;
        internal VertexBuffer VertexBuffer;
        IndexBuffer[] _IndexBuffers;
        GraphicsDevice _device;
 
        internal BufferManager(VertexMultitextured[] vertices, GraphicsDevice device, int indexBufferSize)
        {
            _device = device;
 
            VertexBuffer = new VertexBuffer(device, VertexMultitextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);
 
            _IndexBuffers = new IndexBuffer[]
                    {
                            new IndexBuffer(_device, IndexElementSize.ThirtyTwoBits, indexBufferSize, BufferUsage.WriteOnly),   // 100000
                            new IndexBuffer(_device, IndexElementSize.ThirtyTwoBits, indexBufferSize, BufferUsage.WriteOnly)
                    };
 
        }
 
 
        internal IndexBuffer IndexBuffer
        {
            get { return _IndexBuffers[_active]; }
        }
 
        internal void UpdateIndexBuffer(int[] indices, int indexCount)
        {
            int inactive = _active == 0 ? 1 : 0;           
         
            _IndexBuffers[inactive].SetData(indices, 0, indexCount);
           
        }
 
        internal void SwapBuffer()
        {
            _active = _active == 0 ? 1 : 0;;
        }
    }
}


