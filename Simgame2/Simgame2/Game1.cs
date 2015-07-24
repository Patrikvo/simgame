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


    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private const int mapSize = 320;   // 20 40 80 160 320 640 1280
        private const float rotationSpeed = 0.3f;
        private const float moveSpeed = 60.0f;

        // Rendering objects
        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private Matrix viewMatrix;
        public Matrix projectionMatrix;
        private Effect effect;

        // Camera objects
        private Vector3 cameraPosition = new Vector3(50, 25, -50);
        private float leftrightRot = 2 * MathHelper.Pi;
        private float updownRot = -MathHelper.Pi / 10.0f;

        // Input/output objects
        private MouseState originalMouseState;
        private SpriteFont font;


        // Game Objects
        private WorldMap worldMap;
        private Entity xwing;
        private EntityBuilding building;
        public TextureGenerator textureGenerator;

        
        // test objects

        private EntityBuilding selBuilding;


       

        


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {

            graphics.PreferredBackBufferWidth = 750;
            graphics.PreferredBackBufferHeight = 750;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            base.Initialize();
        }



        private EntityFactory entityFactory;


        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Courier New");
            device = graphics.GraphicsDevice;
            effect = Content.Load<Effect>("Series4Effects");
            
            SetUpCamera();

            // Place mouse position to the center of the screen.
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();



            textureGenerator = new TextureGenerator(this);
           // texture = textureGenerator.GenerateGroundTexture(new Color(124, 124, 124, 1), new Vector3(0,39,39), 512);
            Texture2D texture = textureGenerator.GenerateGroundTexture(new Color(120, 62, 62, 1), new Vector3(20, 20, 20), 512);



            worldMap = new WorldMap(this, mapSize, mapSize);
            worldMap.effect = effect;
            worldMap.projectionMatrix = projectionMatrix;
            worldMap.device = device;

            worldMap.groundTexture = texture;
            worldMap.textureSize = 64;

            worldMap.sandTexture = Content.Load<Texture2D>("sand");
            worldMap.rockTexture = Content.Load<Texture2D>("rock");
            worldMap.snowTexture = Content.Load<Texture2D>("snow");

            worldMap.selectionTexture = textureGenerator.SelectionImage(Color.Yellow, WorldMap.mapCellScale);

            // skydome
            worldMap.skyDome = Content.Load<Model>("dome");
            worldMap.skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();
            worldMap.cloudMap = Content.Load<Texture2D>("cloudMap");



            UpdateViewMatrix();





            Effect modelEffect = Content.Load<Effect>("Effects");

            entityFactory = EntityFactory.CreateFactory(this, this.worldMap);

            EntityBuilding minebuilding = entityFactory.CreateBasicMine(new Vector3(100, 0, -100), true);

            EntityBuilding tower = entityFactory.CreateWindTower(new Vector3(100, 0, -150), true);



            building = new EntityBuilding(this);
            building.LoadModel("testbuilding", modelEffect);
        //    building.location = new Vector3(100, 12, -100);
            building.scale = new Vector3(1f, 1f, 1f);
            building.rotation = new Vector3(0, MathHelper.Pi, 0);
            building.projectionMatrix = projectionMatrix;
            building.texture = Content.Load<Texture2D>("testbuildingtex");
            building.PlaceBuilding(this.worldMap, 50, -50, true);

          //  EntityBuilding minebuilding = new EntityBuilding(this);
           // minebuilding.LoadModel("BasicMine", modelEffect);
          //  minebuilding.scale = new Vector3(5f, 5f, 5f);
         //   minebuilding.rotation = new Vector3(0, MathHelper.Pi, 0);
         //   minebuilding.projectionMatrix = projectionMatrix;
         //   minebuilding.texture = Content.Load<Texture2D>("BasicMineTex");
         //   minebuilding.PlaceBuilding(this.worldMap, 100, -100, true);


            EntityBuilding melterbuilding = new EntityBuilding(this);
            melterbuilding.LoadModel("BasicMelter", modelEffect);
            melterbuilding.scale = new Vector3(5f, 5f, 5f);
            melterbuilding.rotation = new Vector3(0, MathHelper.Pi, 0);
            melterbuilding.projectionMatrix = projectionMatrix;
            melterbuilding.texture = Content.Load<Texture2D>("BasicMelterTex");
            melterbuilding.PlaceBuilding(this.worldMap, 150, -150, true);

            EntityBuilding solarBuilding = new EntityBuilding(this);
            solarBuilding.LoadModel("SolarPlant", modelEffect);
            solarBuilding.scale = new Vector3(5f, 5f, 5f);
            solarBuilding.rotation = new Vector3(0, MathHelper.Pi, 0);
            solarBuilding.projectionMatrix = projectionMatrix;
            solarBuilding.texture = Content.Load<Texture2D>("SolarPlantTex");
            solarBuilding.PlaceBuilding(this.worldMap, 200, -200, true);


            





            selBuilding = new EntityBuilding(this);
            selBuilding.LoadModel("testbuilding", modelEffect);
            selBuilding.location = new Vector3(50, 12, -50);
            selBuilding.scale = new Vector3(1f, 1f, 1f);
            selBuilding.rotation = new Vector3(0, MathHelper.Pi, 0);
            selBuilding.projectionMatrix = projectionMatrix;
            selBuilding.texture = Content.Load<Texture2D>("testbuildingtex");



            EntityBuilding[] buildings = new EntityBuilding[10];

            for (int i = 0; i < 0; i++)
            {
                buildings[i] = new EntityBuilding(this);
                buildings[i].LoadModel("testbuilding", modelEffect);
                buildings[i].location = new Vector3(60 + (i * 20), 12, -(60 + (i * 20)));
                buildings[i].scale = new Vector3(1f, 1f, 1f);
                buildings[i].rotation = new Vector3(0, 0, 0);
                buildings[i].projectionMatrix = projectionMatrix;
                buildings[i].texture = Content.Load<Texture2D>("testbuildingtex");
                buildings[i].PlaceBuilding(this.worldMap, 60+(i*20), -(60 + (i*20)),true );
            }

        }



        protected override void UnloadContent()

        {
            // TODO: Unload any non ContentManager content here
        }




        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference);

            entityFactory.Update(gameTime);


            // place building code
            selBuilding.RemoveBuilding(worldMap);
            if (isPlacingBuilding)
            {
                

                Vector3 offset = Vector3.Transform(new Vector3(0, 0, -50), Matrix.CreateRotationY(leftrightRot));
                selBuilding.location = cameraPosition + offset;

                selBuilding.PlaceBuilding(worldMap, false);

            }


            base.Update(gameTime);
        }

   


        private int fps;
        private SpriteBatch spriteBatch;
        protected override void Draw(GameTime gameTime)
        {
            fps = (int)(1000 / gameTime.ElapsedGameTime.Milliseconds);


            
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;   // fixes the Z-buffer. 


            worldMap.Draw(viewMatrix, cameraPosition);


            // Draws stats
            spriteBatch = new SpriteBatch(this.device);
            spriteBatch.Begin();
            Vector2 pos = new Vector2(20, 20);
            Rectangle rect = new Rectangle(20, 50, 32, 32);
            spriteBatch.DrawString(font, "fps: " + fps.ToString() + " - " + worldMap.GetStats(), pos,Color.White);
            spriteBatch.Draw(worldMap.groundTexture, rect, Color.White);
            spriteBatch.End();
            


            base.Draw(gameTime);
        }



        bool isPlacingBuilding = false;
        bool mouseButtonDown = false;
        bool ButtonSpaceDown = false;

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




            if (keyState.IsKeyDown(Keys.Space))
                ButtonSpaceDown = true;

            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                isPlacingBuilding = !isPlacingBuilding;
                ButtonSpaceDown = false;
            }

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                mouseButtonDown = true;
            }

            if (currentMouseState.LeftButton == ButtonState.Released && mouseButtonDown == true)
            {
                mouseButtonDown = false;
                if (isPlacingBuilding)
                {
                    isPlacingBuilding = false;
                    selBuilding.RemoveBuilding(worldMap);
                    selBuilding.PlaceBuilding(worldMap, true);

                    selBuilding = new EntityBuilding(selBuilding);
                }
            }

         

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

            worldMap.updateCameraRay(new Ray(cameraPosition, cameraRotatedTarget));

        }






    }
}
