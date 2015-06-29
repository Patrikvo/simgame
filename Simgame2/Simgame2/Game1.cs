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

    public struct VertexPositionNormalColored : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        

        public static int SizeInBytes = 7 * 4;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );


        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

    }

    

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private const int mapSize = 160;   // 20 40 80 160 320 640 1280


        GraphicsDeviceManager graphics;
//        SpriteBatch spriteBatch;
        GraphicsDevice device;

        Vector3 cameraPosition = new Vector3(50, 50, -50);
        float leftrightRot = 2*MathHelper.Pi; // .PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 60.0f;
        MouseState originalMouseState;

        private WorldMap worldMap;

        Model xwingModel;
        Vector3 xwingLocation;

        SpriteFont font;

        private Model LoadModel(string assetName)
        {

            Model newModel = Content.Load<Model>(assetName); foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
            return newModel;
        }


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();



            // TODO: Add your initialization logic here
            
            
            


            base.Initialize();
        }



//        Texture2D mapimage;
        //Rectangle screenRectangle;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;
        private Effect effect;
        private VertexBuffer terrainVertexBuffer;
        private IndexBuffer terrainIndexBuffer;


        private TextureGenerator textureGenerator;
        private Texture2D texture;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Courier New");
            


            textureGenerator = new TextureGenerator(this);
            texture = textureGenerator.GenerateGroundTexture(new Color(0,200,0,1), 500);

            device = graphics.GraphicsDevice;
            //effect = Content.Load<Effect>("Series4Effects");
            effect = Content.Load<Effect>("Effects"); 
            SetUpCamera();

            worldMap = new WorldMap(this, mapSize, mapSize);  
            UpdateViewMatrix();


            xwingModel = LoadModel("xwing");
            xwingLocation = new Vector3(50, 12, -150);

            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);

            originalMouseState = Mouse.GetState();

        }

        float speed = 10;
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        int fps;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference);

            fps = (int)(1 / timeDifference);

            

            float y = xwingLocation.Y + speed * timeDifference;

            if (y < -50 || y > 50) { speed = - speed;}

            xwingLocation = new Vector3(xwingLocation.X, y, xwingLocation.Z);



            base.Update(gameTime);
        }

        private void CopyToTerrainBuffers(VertexPositionNormalColored[] vertices, Int16[] indices)
        {

            terrainVertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalColored), vertices.Length, BufferUsage.WriteOnly);

            terrainVertexBuffer.SetData(vertices);

            terrainIndexBuffer = new IndexBuffer(device, typeof(Int16), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);


        }

        SpriteBatch spriteBatch;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch = new SpriteBatch(this.device);
            GraphicsDevice.Clear(Color.Black);
 
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;   // fixes the Z-buffer. 

            DrawTerrain(viewMatrix);

            DrawModel(viewMatrix);

            spriteBatch.Begin();
            Vector2 pos = new Vector2(20, 20);
            //spriteBatch.DrawString(font, "fps: " + fps.ToString(), pos,Color.White);
            spriteBatch.Draw(texture, pos, Color.White);
            spriteBatch.End();
            


            base.Draw(gameTime);
        }



        private void DrawTerrain(Matrix currentViewMatrix)
        {
            effect.CurrentTechnique = effect.Techniques["Colored"];
            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(currentViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.4f);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

            effect.CurrentTechnique.Passes[0].Apply();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                device.SetVertexBuffer(terrainVertexBuffer);
                device.Indices = terrainIndexBuffer;

                    

                int noVertices = terrainVertexBuffer.VertexCount; 
                int noTriangles = terrainIndexBuffer.IndexCount / 3;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);
            }
        }





        private void DrawModel(Matrix currentViewMatrix)
        {
            Matrix worldMatrix = Matrix.CreateScale(0.05f, 0.05f, 0.05f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(xwingLocation);

            Matrix[] xwingTransforms = new Matrix[xwingModel.Bones.Count];
            xwingModel.CopyAbsoluteBoneTransformsTo(xwingTransforms);

            

            foreach (ModelMesh mesh in xwingModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Colored"];
                    currentEffect.Parameters["xWorld"].SetValue(xwingTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                }
                mesh.Draw();
            }
        }






        private void SetUpCamera()
        {
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -1));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);

            
        }

        private void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            float yDifference = 0;

            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                 yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
                UpdateViewMatrix();
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Escape))
                    this.Exit();

            if (keyState.IsKeyDown(Keys.T))
            {
                worldMap.useTree = true;
            }

            if (keyState.IsKeyDown(Keys.Y))
            {
                worldMap.useTree = false ;
            }

            if (keyState.IsKeyDown(Keys.P))
            {
                worldMap.debugPrintTree();
            }


            if (keyState.IsKeyDown(Keys.Space))
                yDifference = yDifference;


         //   if (keyState.IsKeyDown(Keys.Z))
         //       moveVector += new Vector3(0, -1, 0);

            if (yDifference != 0)
            {
                moveVector += new Vector3(0, 0, yDifference);
            }
          //  if (yDifference > 0)
           // {
            //    moveVector += new Vector3(0, 0, yDifference);
            //}

         

            AddToCameraPosition(moveVector * amount);

            BoundingFrustum frustum = new BoundingFrustum(viewMatrix * projectionMatrix);
            worldMap.frustum = frustum;

            worldMap.GenerateView();

            if (worldMap.vertices.Length > 0 && worldMap.indices.Length > 0)
            {
                CopyToTerrainBuffers(worldMap.vertices, worldMap.indices);
            }


        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            //Matrix cameraRotation = Matrix.CreateRotationX(-MathHelper.Pi/6) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);



        }

    }
}
