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

    public struct VertexMultitextured : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = (3 + 3 + 4 + 4) * sizeof(float);
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
     (
         new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
         new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
         new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0 ),
         new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 )
     );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }



    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private const int mapSize = 320;   // 20 40 80 160 320 640 1280


        GraphicsDeviceManager graphics;
//        SpriteBatch spriteBatch;
        GraphicsDevice device;

        Vector3 cameraPosition = new Vector3(50, 25, -50);
        float leftrightRot = 2*MathHelper.Pi; // .PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 60.0f;
        MouseState originalMouseState;

        private WorldMap worldMap;
        private Entity xwing;

        SpriteFont font;
/*
        private Model LoadModel(string assetName)
        {
            Effect modelEffect = Content.Load<Effect>("Effects");
            Model newModel = Content.Load<Model>(assetName); foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = modelEffect.Clone();
            return newModel;
        }
*/

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

            graphics.PreferredBackBufferWidth = 750;
            graphics.PreferredBackBufferHeight = 750;
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
            


            

            device = graphics.GraphicsDevice;
            effect = Content.Load<Effect>("Series4Effects");
          

            
            SetUpCamera();

            textureGenerator = new TextureGenerator(this);



           // texture = textureGenerator.GenerateGroundTexture(new Color(124, 124, 124, 1), new Vector3(0,39,39), 512);
            texture = textureGenerator.GenerateGroundTexture(new Color(120, 62, 62, 1), new Vector3(20, 20, 20), 512);



            worldMap = new WorldMap(this, mapSize, mapSize);
            worldMap.effect = effect;
            worldMap.projectionMatrix = projectionMatrix;
            worldMap.device = device;

            worldMap.groundTexture = texture;
            worldMap.textureSize = 64;

            worldMap.sandTexture = Content.Load<Texture2D>("sand");
            worldMap.rockTexture = Content.Load<Texture2D>("rock");
            worldMap.snowTexture = Content.Load<Texture2D>("snow");

            // skydome
            worldMap.skyDome = Content.Load<Model>("dome");
            worldMap.skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();
            worldMap.cloudMap = Content.Load<Texture2D>("cloudMap");


            UpdateViewMatrix();
            Effect modelEffect = Content.Load<Effect>("Effects");

            xwing = new Entity(this);

            xwing.LoadModel("xwing", modelEffect);
            xwing.location = new Vector3(50, 12, -150);
            xwing.scale = new Vector3(0.05f, 0.05f, 0.05f);
            xwing.rotation = new Vector3(0, MathHelper.Pi, 0);
            xwing.projectionMatrix = projectionMatrix;


       //     xwingModel = LoadModel("xwing");
            //xwingLocation = new Vector3(50, 12, -150);

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

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference);

            

            

            float y = xwing.location.Y + speed * timeDifference;

            if (y < -50 || y > 50) { speed = - speed;}

            xwing.location = new Vector3(xwing.location.X, y, xwing.location.Z);



            base.Update(gameTime);
        }

        private void CopyToTerrainBuffers(VertexMultitextured[] vertices, Int16[] indices)
        {

            terrainVertexBuffer = new VertexBuffer(device, typeof(VertexMultitextured), vertices.Length, BufferUsage.WriteOnly);

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
            fps = (int)(1000 / gameTime.ElapsedGameTime.Milliseconds);


            spriteBatch = new SpriteBatch(this.device);
            GraphicsDevice.Clear(Color.Black);
 
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;   // fixes the Z-buffer. 

            worldMap.Draw(viewMatrix, cameraPosition);

            xwing.Draw(viewMatrix);

            

            spriteBatch.Begin();
            Vector2 pos = new Vector2(20, 20);
            Rectangle rect = new Rectangle(20, 50, 32, 32);
            spriteBatch.DrawString(font, "fps: " + fps.ToString() + " - " + worldMap.GetStats(), pos,Color.White);
            spriteBatch.Draw(worldMap.groundTexture, rect, Color.White);
            
            spriteBatch.End();
            


            base.Draw(gameTime);
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
               // yDifference = yDifference;


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
