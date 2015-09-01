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
        private const int mapNumCellsPerSide = 320;   // 20 40 80 160 320 640 1280

        // Rendering objects
        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private Effect effect;

        // Camera objects
        private Camera PlayerCamera;


        // Input/output objects
        private MouseState originalMouseState;
        private SpriteFont font;

        // GUI

        GUI HUD_overlay;


        // Game Objects
        private WorldMap worldMap;
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
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 900;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            HUD_overlay = new GUI(this);
            base.Initialize();
        }



        private EntityFactory entityFactory;

        private bool doneLoading = false;

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Courier New");
            device = graphics.GraphicsDevice;
            effect = Content.Load<Effect>("Series4Effects");


            
            worldMap = new WorldMap(this, mapNumCellsPerSide, mapNumCellsPerSide, effect, device);
            PlayerCamera = new Camera(device.Viewport.AspectRatio);
            PlayerCamera.worldMap = worldMap;



            // Place mouse position to the center of the screen.
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            HUD_overlay.AddButton(Content.Load<Texture2D>("GUI\\test"), Content.Load<Texture2D>("GUI\\test2"));
            HUD_overlay.AddButton(Content.Load<Texture2D>("GUI\\test"), Content.Load<Texture2D>("GUI\\test2"));
            HUD_overlay.AddButton(Content.Load<Texture2D>("GUI\\test"), Content.Load<Texture2D>("GUI\\test2"));

            textureGenerator = new TextureGenerator(this);
           // texture = textureGenerator.GenerateGroundTexture(new Color(124, 124, 124, 1), new Vector3(0,39,39), 512);
            Texture2D texture = textureGenerator.GenerateGroundTexture(new Color(120, 62, 62, 1), new Vector3(20, 20, 20), 512);


            worldMap.waterBumpMap = Content.Load<Texture2D>("waterbump");


            worldMap.groundTexture = Content.Load<Texture2D>("Textures\\tex1");
            worldMap.sandTexture = Content.Load<Texture2D>("Textures\\tex0");
            worldMap.rockTexture = Content.Load<Texture2D>("Textures\\tex2");
            worldMap.snowTexture = Content.Load<Texture2D>("Textures\\tex3");

            worldMap.textureSize = 512;

            worldMap.selectionTexture = textureGenerator.SelectionImage(Color.Yellow, WorldMap.mapCellScale);

            // skydome
            worldMap.LoadSkyDome(Content.Load<Model>("dome"));


            PlayerCamera.UpdateViewMatrix();



            entityFactory = EntityFactory.CreateFactory(this, this.worldMap, PlayerCamera.projectionMatrix);

            EntityBuilding minebuilding = entityFactory.CreateBasicMine(new Vector3(100, 0, -100), true);

            EntityBuilding tower = entityFactory.CreateWindTower(new Vector3(100, 0, -150), true);

            EntityBuilding melter = entityFactory.CreateBasicMelter(new Vector3(150, 0, -150), true);

            EntityBuilding solar = entityFactory.CreateBasicSolarPlant(new Vector3(200, 0, -200), true);



            selBuilding = new EntityBuilding(this);
            selBuilding.LoadModel("testbuilding", effect);
            selBuilding.location = new Vector3(50, 12, -50);
            selBuilding.scale = new Vector3(1f, 1f, 1f);
            selBuilding.rotation = new Vector3(0, MathHelper.Pi, 0);
            selBuilding.projectionMatrix = PlayerCamera.projectionMatrix;
            selBuilding.AddTexture(Content.Load<Texture2D>("testbuildingtex"));



            doneLoading = true;
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

            PlayerCamera.AdjustCameraAltitude(gameTime);




            ProcessInput(timeDifference);

            entityFactory.Update(gameTime);


            // place building code
            selBuilding.RemoveBuilding(worldMap);
            if (isPlacingBuilding)
            {
                

                Vector3 offset = Vector3.Transform(new Vector3(0, 0, -50), Matrix.CreateRotationY(PlayerCamera.leftrightRot));
                selBuilding.location = PlayerCamera.GetCameraPostion() + offset;

                selBuilding.PlaceBuilding(worldMap, false);

            }


            base.Update(gameTime);
        }





        public Texture2D debugImg = null;
        bool showDebugImg = false;
        private int fps = 0;
        private float fps_avg = 0;
        private int fps_sampleSize = 10;
        private int fps_SampleNum = 10;
        private SpriteBatch spriteBatch;

        protected override void Draw(GameTime gameTime)
        {
            if (!doneLoading) { return; }

            if (gameTime.ElapsedGameTime.Milliseconds != 0)
            {
               // fps = (int)(1000 / gameTime.ElapsedGameTime.Milliseconds);
                fps_avg += (1000 / gameTime.ElapsedGameTime.Milliseconds);
                fps_SampleNum--;
                if (fps_SampleNum <= 0)
                {
                    fps = (int)(fps_avg / fps_sampleSize);
                    fps_SampleNum = fps_sampleSize;
                    fps_avg = 0;
                }

            }

            
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;   // fixes the Z-buffer. 


            worldMap.Draw(PlayerCamera, gameTime);


            // Draws stats
            spriteBatch = new SpriteBatch(this.device);
            spriteBatch.Begin();
            Vector2 pos = new Vector2(20, 20);
            Vector2 pos2 = new Vector2(20, 60);
            Rectangle rect = new Rectangle(20, 50, 400, 400);
            spriteBatch.DrawString(font, "fps: " + fps.ToString() + " - " + worldMap.GetStats(), pos,Color.White);
            spriteBatch.DrawString(font, "(" + this.PlayerCamera.GetCameraPostion().X + ", " + this.PlayerCamera.GetCameraPostion().Y + ", " + this.PlayerCamera.GetCameraPostion().Z + ")", pos2, Color.White);
            if (debugImg != null && showDebugImg == true)
            {
                spriteBatch.Draw(debugImg, rect, Color.White);
            }
            spriteBatch.End();

            if (!CaptureMouse)
            {
                HUD_overlay.Draw(gameTime, this.device);
            }

            base.Draw(gameTime);
        }



        bool isPlacingBuilding = false;
        bool mouseLeftButtonDown = false;
        bool mouseRightButtonDown = false;
        bool ButtonSpaceDown = false;
        bool ButtonXDown = false;

        bool CaptureMouse = true;

        private void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            float yDifference = 0;

            if (currentMouseState != originalMouseState && CaptureMouse)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                 yDifference = currentMouseState.Y - originalMouseState.Y;
                 PlayerCamera.leftrightRot -= Camera.rotationSpeed * xDifference * amount;
                 PlayerCamera.updownRot -= Camera.rotationSpeed * yDifference * amount;
                Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
                PlayerCamera.UpdateViewMatrix();
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

            if (keyState.IsKeyDown(Keys.Pause))
            {
                int gg = 0;
            }



            if (keyState.IsKeyDown(Keys.Space))
                ButtonSpaceDown = true;

            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                isPlacingBuilding = !isPlacingBuilding;
                ButtonSpaceDown = false;
            }

            if (keyState.IsKeyDown(Keys.X))
                ButtonXDown = true;

            if (keyState.IsKeyUp(Keys.X) && ButtonXDown == true)
            {
                showDebugImg = !showDebugImg;
                ButtonXDown = false;
            }

         

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                mouseLeftButtonDown = true;
            }

            if (currentMouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown == true)
            {
                mouseLeftButtonDown = false;
                if (isPlacingBuilding)
                {
                    isPlacingBuilding = false;
                    selBuilding.RemoveBuilding(worldMap);
                    selBuilding.PlaceBuilding(worldMap, true);

                    selBuilding = new EntityBuilding(selBuilding);
                }
            }

            if (currentMouseState.RightButton == ButtonState.Pressed)
            {
                mouseRightButtonDown = true;
            }

            if (currentMouseState.RightButton == ButtonState.Released && mouseRightButtonDown == true)
            {
                mouseRightButtonDown = false;

                CaptureMouse = !CaptureMouse;

                if (CaptureMouse)
                {
                    Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
                    originalMouseState = Mouse.GetState();
                    this.IsMouseVisible = false;
                }
                else
                {
                    this.IsMouseVisible = true;
                }
            }

            if (!CaptureMouse)
            {

                HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, currentMouseState.LeftButton == ButtonState.Pressed);
            }
            else
            {
                HUD_overlay.Update(0, 0, false);
            }


         
            PlayerCamera.AddToCameraPosition(moveVector * amount);
            


        }

 







    }
}
