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
        public Color Color1;
        public Color Color2;
        public Vector3 Normal;

        public VertexPositionNormalColored(Vector3 position, Color color1, Color color2, Vector3 normal)
        {
            this.Position = position;
            this.Color1 = color1;
            this.Color2 = color2;
            this.Normal = normal;
        }

        public VertexPositionNormalColored(Vector3 position, Color color1, Color color2)
        {
            this.Position = position;
            this.Color1 = color1;
            this.Color2 = color2;
            this.Normal = new Vector3(0f, 0f, 0f);
        }

        public static int SizeInBytes = 7 * 4;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
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
        private const int mapSize = 80;   // 20 40 80 160 320 640 1280
        private const float rotationSpeed = 0.3f;
        private const float moveSpeed = 60.0f;
        private const float riseSpeed = 60.0f;
        private const float dropSpeed = 30.0f;

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
       // private Entity xwing;
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

            worldMap.Initialize();
            worldMap.waterBumpMap = Content.Load<Texture2D>("waterbump");


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

            EntityBuilding melter = entityFactory.CreateBasicMelter(new Vector3(150, 0, -150), true);

            EntityBuilding solar = entityFactory.CreateBasicSolarPlant(new Vector3(200, 0, -200), true);



            selBuilding = new EntityBuilding(this);
            selBuilding.LoadModel("testbuilding", modelEffect);
            selBuilding.location = new Vector3(50, 12, -50);
            selBuilding.scale = new Vector3(1f, 1f, 1f);
            selBuilding.rotation = new Vector3(0, MathHelper.Pi, 0);
            selBuilding.projectionMatrix = projectionMatrix;
            selBuilding.AddTexture(Content.Load<Texture2D>("testbuildingtex"));




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

            // keeps camera at a set height above the terrain.
            int intendedCameraHeight = (worldMap.getCellFromWorldCoor(cameraPosition.X, -cameraPosition.Z)) + CameraHeightOffset;

            if (cameraHeight < intendedCameraHeight)
            {
                cameraHeight += riseSpeed * gameTime.ElapsedGameTime.TotalSeconds;
                if (cameraHeight > intendedCameraHeight)
                {
                    cameraHeight = intendedCameraHeight;
                }
            }
            else if (cameraHeight > intendedCameraHeight)
            {
                cameraHeight -= dropSpeed * gameTime.ElapsedGameTime.TotalSeconds;
                if (cameraHeight < intendedCameraHeight)
                {
                    cameraHeight = intendedCameraHeight;
                }
            }




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


        public Texture2D debugImg = null;

        private int fps = 0;
        private SpriteBatch spriteBatch;
        protected override void Draw(GameTime gameTime)
        {
            if (gameTime.ElapsedGameTime.Milliseconds != 0)
            {
                fps = (int)(1000 / gameTime.ElapsedGameTime.Milliseconds);
            }

            
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;   // fixes the Z-buffer. 


            worldMap.Draw(viewMatrix, cameraPosition, gameTime);


            // Draws stats
            spriteBatch = new SpriteBatch(this.device);
            spriteBatch.Begin();
            Vector2 pos = new Vector2(20, 20);
            Rectangle rect = new Rectangle(20, 50, 100, 100);
            spriteBatch.DrawString(font, "fps: " + fps.ToString() + " - " + worldMap.GetStats(), pos,Color.White);
            if (debugImg != null)
            {
                spriteBatch.Draw(debugImg, rect, Color.White);
            }
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
        private const int CameraHeightOffset = 25;
        private double cameraHeight;
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;

            float camX = cameraPosition.X;
            float camZ = cameraPosition.Z;

            if (camX < 0) { camX = 0;  }
            if (camX > (worldMap.getMapWidth() * WorldMap.mapCellScale)) { camX = (worldMap.getMapWidth() * WorldMap.mapCellScale); }

            if (camZ > 0) { camZ = 0; }
            if (camZ < -((worldMap.getMapHeight() * WorldMap.mapCellScale)-1)) { camZ = -((worldMap.getMapHeight() * WorldMap.mapCellScale) -1 ); }

            cameraPosition = new Vector3(camX, (float)cameraHeight, camZ);
            //cameraPosition = new Vector3(cameraPosition.X, (float)cameraHeight, cameraPosition.Z);
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


            Vector3 reflCameraPosition = cameraPosition;
            reflCameraPosition.Y = -cameraPosition.Y + (WorldMap.waterHeight*2);
            Vector3 reflTargetPos = cameraFinalTarget;
            reflTargetPos.Y = -cameraFinalTarget.Y + (WorldMap.waterHeight*2);
        //    reflTargetPos.Y = -cameraFinalTarget.Y - (WorldMap.waterHeight * 2);


 
            Vector3 forwardVector = reflTargetPos - reflCameraPosition;
            Vector3 sideVector = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation); 
            Vector3 reflectionCamUp = Vector3.Cross(sideVector, forwardVector);
            worldMap.reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, reflectionCamUp);

           
            

        }






    }
}
