using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.LODTerrain
{
    public class QuadTree
    {
        public QuadTree(Vector4 mapSize, GraphicsDevice device, GameSession.GameStorage storage)
        {
         //   this.Cull = false;
            this.MinimumDepth = 8;

            int bufferSize = (int)((mapSize.X + 1) * (mapSize.Y + 1)) * 3;
            Device = device;
          //  _position = position;
          //  _topNodeSize =  heightMap.Width - 1;
            _topNodeSize = (int)mapSize.X - 1;

            if (storage == null)
            {
                _vertices = new TreeVertexCollection(mapSize);
            }
            else
            {
                _vertices = new TreeVertexCollection(storage);
            }
            _buffers = new BufferManager(_vertices.Vertices, device, bufferSize);
            _rootNode = new QuadNode(NodeType.FullNode, _topNodeSize, 1, null, this, 0);
         //   View = viewMatrix;
         //   Projection = projectionMatrix;

          //  ViewFrustrum = new BoundingFrustum(viewMatrix * projectionMatrix);

            //Construct an array large enough to hold all of the indices we'll need.
          //  Indices = new int[((heightMap.Width + 1) * (heightMap.Height + 1)) * 3];
            //Indices = new int[(int)((mapSize.X + 1) * (mapSize.Y + 1)) * 3];
            Indices = new int[bufferSize];
        }

        public void Store(GameSession.GameStorage storage)
        {
            this._vertices.Store(storage);
        }

        float LastCameraLeftRightRot = 0;
        float LastCameraUpDownRot = 0;

        public void Update(GameTime gameTime, Camera camera)
        {
        

            //Only update if the camera position or rotation has changed
            if (camera.GetCameraPostion() == _lastCameraPosition &&
                LastCameraLeftRightRot == camera.leftrightRot &&
                LastCameraUpDownRot == camera.updownRot)
            {
                return;
            }

            //   this.ViewFrustrum = viewFrustrum;
            //this.ViewFrustrum = new BoundingFrustum(camera.viewMatrix * camera.projectionMatrix);
            this.ViewFrustrum = new BoundingFrustum(camera.viewMatrixBackShifted * camera.BigProjectionMatrix);

      //      Effect.View = View;
      //      Effect.Projection = Projection;

            _lastCameraPosition = camera.GetCameraPostion();
            LastCameraLeftRightRot = camera.leftrightRot;
            LastCameraUpDownRot = camera.updownRot;

            IndexCount = 0;

            _rootNode.Merge();
            _rootNode.EnforceMinimumDepth();
            

          //  _activeNode = _rootNode.DeepestNodeWithPoint(camera.GetCameraPostion());
            _activeNode = _rootNode.DeepestNodeWithPoint(camera.GetPointBehindCamera(100));

            if (_activeNode != null)
            {
                _activeNode.Split();
            }

            _rootNode.SetActiveVertices();

            _buffers.UpdateIndexBuffer(Indices, IndexCount);
            _buffers.SwapBuffer();
        }


        public void Draw(GameTime gameTime, GraphicsDevice device)
        {
         /*   device.SetVertexBuffer(_buffers.VertexBuffer);
            device.Indices = _buffers.IndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertices.Vertices.Length, 0, IndexCount / 3);
            }
           */ 

        }

        public VertexBuffer GetVertexBuffer() { return _buffers.VertexBuffer; }
        public IndexBuffer GetIndexBuffer() { return _buffers.IndexBuffer; }

        internal void UpdateBuffer(int vIndex)
        {
            Indices[IndexCount] = vIndex;
            IndexCount++;
        }


        public int MinimumDepth = 0;// 6;

        public bool Cull { get; set; }

        private QuadNode _activeNode;

        private QuadNode _rootNode;
        private TreeVertexCollection _vertices;
        private BufferManager _buffers;
     //   private Vector3 _position;
        private int _topNodeSize;
 
      //  private Vector3 _cameraPosition;
        private Vector3 _lastCameraPosition;
 
        public int[] Indices;
 
    //    public Matrix View;
     //   public Matrix Projection;
 
        public GraphicsDevice Device;
 
        public int TopNodeSize { get { return _topNodeSize; } }
        public QuadNode RootNode { get { return _rootNode; } }
        public TreeVertexCollection Vertices { get { return _vertices; } }
   
 
        internal BoundingFrustum ViewFrustrum { get; set; }

        public int IndexCount { get; set; }
    }
}

