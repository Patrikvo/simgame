using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Simgame2
{
    public class Billboard: Entity
    {
        public Billboard(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Width = 25;
            this.Height = 25;
            this.Show = true;
            BillboardEffect = this.RunningGameSession.effect;
        }

        public Effect BillboardEffect { get; set; }

        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            // TODO IMPLEMENT
            return false;
        }


        public void loadTexture(Texture2D texture, Vector3 location)
        {
            this.BillboardBackGroundTexture = texture;
            this.location = new Vector3(location.X, location.Y + this.Height, location.Z);
            
            CreateBillboardVerticesFromList();
        }


        public void loadTexture(Texture2D texture)
        {
            this.BillboardBackGroundTexture = texture;
            CreateBillboardVerticesFromList();
        }


        public void SetTexture(Texture2D tex)
        {
            this.billboardTexture = tex;
        }
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        

        public void Draw(Camera playerCamera)
        {
            float dist = DistanceToCamera(playerCamera);
            if (Show &&  dist < 200 && dist > 50)
            {
                DrawBillboards(playerCamera);
            }
        }

        private float DistanceToCamera(Camera playerCamera)
        {
            float distance;
            Vector3 camPos = playerCamera.GetCameraPostion();
            Vector3 loc = this.location;
            Vector3.Distance(ref camPos, ref loc, out distance);

            return distance;
        }

        private void DrawBillboards(Camera playerCamera)
        {

            Matrix billboardMatrix = Matrix.CreateConstrainedBillboard(this.location,
                playerCamera.GetCameraPostion(), Vector3.Up, playerCamera.LookAt, Vector3.UnitZ);

            Matrix offCenter = Matrix.CreateTranslation(playerCamera.sideVector * (offset + (this.Width/2)));


            this.BillboardEffect.CurrentTechnique = this.effect.Techniques["CylBillboard"];
            this.BillboardEffect.Parameters["xWorld"].SetValue(billboardMatrix * offCenter);
            this.BillboardEffect.Parameters["xView"].SetValue(playerCamera.viewMatrix);
            this.BillboardEffect.Parameters["xProjection"].SetValue(playerCamera.projectionMatrix);
            this.BillboardEffect.Parameters["xCamPos"].SetValue(playerCamera.GetCameraPostion());

            this.BillboardEffect.Parameters["xBillboardTexture"].SetValue(billboardTexture);

            

            RasterizerState prevRasterizerState = this.device.RasterizerState;
       
            BlendState prevBlendState = this.device.BlendState;

            this.device.BlendState = BlendState.NonPremultiplied;
            this.device.RasterizerState = RasterizerState.CullNone;
            this.device.DepthStencilState = DepthStencilState.DepthRead;

            foreach (EffectPass pass in this.BillboardEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.device.SetVertexBuffer(treeVertexBuffer);
                this.device.Indices = this.indexBuffer;


                this.device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, this.treeVertexBuffer.VertexCount, 0, 2);
                
            }

            this.device.BlendState = BlendState.Opaque;

            this.device.RasterizerState = prevRasterizerState;
            this.device.DepthStencilState = DepthStencilState.Default;
        }

        private void CreateBillboardVerticesFromList()
        {

            float x = this.Width * 0.5f;
            float z = this.Height * 0.5f;


            VertexPositionTexture[] billboardVertices =
            {
                new VertexPositionTexture(new Vector3(-x, -z, 0.0f), new Vector2(1.0f, 1.0f)),  // 0
                new VertexPositionTexture(new Vector3( x, -z, 0.0f), new Vector2(0.0f, 1.0f)),  // 1
                new VertexPositionTexture(new Vector3(-x,  z, 0.0f), new Vector2(1.0f, 0.0f)),  // 2
                new VertexPositionTexture(new Vector3( x,  z, 0.0f), new Vector2(0.0f, 0.0f)),  // 3
            };


            treeVertexBuffer = new VertexBuffer(this.device, typeof(VertexPositionTexture), billboardVertices.Length, BufferUsage.WriteOnly);
            treeVertexBuffer.SetData(billboardVertices);


            short[] indices =
            {
                0, 3, 2,
                0, 1, 3
            };

            indexBuffer = new IndexBuffer(this.device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public Texture2D billboardTexture;
        public Texture2D BillboardBackGroundTexture;
        private VertexBuffer treeVertexBuffer;
        private IndexBuffer indexBuffer;

        public float Width { get; set; }
        public float  Height { get; set; }
        public float offset { get; set; }

        public bool Show { get; set; }


    }
}
