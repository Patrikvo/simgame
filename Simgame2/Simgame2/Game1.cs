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
        public GraphicsDevice device;
        public Effect effect;

        // Camera objects
        public Camera PlayerCamera;


        // Input/output objects
        public SpriteFont font;

        // GUI

        public GUI HUD_overlay;


        // Game Objects
        public WorldMap worldMap;
        public TextureGenerator textureGenerator;


        public GameStates.GameState CurrentGameState;
        public GameStates.FreeLook FreeLookState;
        public GameStates.MousePointerLook MousePointerLookState;
        public GameStates.PlaceBuilding PlaceBuildingState;


        public Simulation.Simulator simulator;

        // test objects

        public EntityBuilding selBuilding;

        
       

        


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            simulator = new Simulation.Simulator(this);
            FreeLookState = new GameStates.FreeLook(this);
            MousePointerLookState = new GameStates.MousePointerLook(this);
            PlaceBuildingState = new GameStates.PlaceBuilding(this);

            
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



        public EntityFactory entityFactory;

        private bool doneLoading = false;



        public void PlaceBuilding()
        {
            ChangeGameState(PlaceBuildingState);
        }


        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Courier New");
            device = graphics.GraphicsDevice;
            effect = Content.Load<Effect>("Series4Effects");


            PlayerCamera = new Camera(device.Viewport.AspectRatio);
            worldMap = new WorldMap(this, mapNumCellsPerSide, mapNumCellsPerSide, effect, device);
            
            PlayerCamera.worldMap = worldMap;

            PlayerCamera.DrawDistance = 300.0f;

            HUD_overlay.PreloadImages(this.Content);

            HUD_overlay.ConstructButtons();

            


            textureGenerator = new TextureGenerator(this);
           // texture = textureGenerator.GenerateGroundTexture(new Color(124, 124, 124, 1), new Vector3(0,39,39), 512);
            Texture2D texture = textureGenerator.GenerateGroundTexture(new Color(120, 62, 62, 1), new Vector3(20, 20, 20), 512);


            worldMap.GetRenderer().waterBumpMap = Content.Load<Texture2D>("waterbump");

            worldMap.GetRenderer().Textures = new Texture2D[5];
            worldMap.GetRenderer().Textures[0] = Content.Load<Texture2D>("Textures\\tex1");
            worldMap.GetRenderer().Textures[1] = Content.Load<Texture2D>("Textures\\tex0");
            worldMap.GetRenderer().Textures[2] = Content.Load<Texture2D>("Textures\\tex2");
            worldMap.GetRenderer().Textures[3] = Content.Load<Texture2D>("Textures\\tex3");
            worldMap.GetRenderer().Textures[4] = Content.Load<Texture2D>("Textures\\tex1");


            worldMap.selectionTexture = textureGenerator.SelectionImage(Color.Yellow, 5); //WorldMap.mapCellScale);

            // skydome
            worldMap.GetRenderer().LoadSkyDome(Content.Load<Model>("dome"));


            PlayerCamera.UpdateViewMatrix();



            entityFactory = EntityFactory.CreateFactory(this, this.worldMap, PlayerCamera.projectionMatrix);

  
            doneLoading = true;
            ChangeGameState(FreeLookState);
        }



        protected override void UnloadContent()

        {
            // TODO: Unload any non ContentManager content here
        }


        public Vector3 markerLocation;

        protected override void Update(GameTime gameTime)
        {
            stopwatch.Reset();
            stopwatch.Start();
            simulator.Update(gameTime);
            this.CurrentGameState.Update(gameTime);

            base.Update(gameTime);
            stopwatch.Stop();
        }





        public Texture2D debugImg = null;
        public bool showDebugImg = false;
        private int fps = 0;
        private float fps_avg = 0;
        private int fps_sampleSize = 10;
        private int fps_SampleNum = 10;

        private float drawTime_avg = 0;
        private int drawTime_sampleSize = 10;
        private int drawTime_SampleNum = 10;
        private int drawtime = 0;
        private SpriteBatch spriteBatch;

        Vector2 pos = new Vector2(20, 20);
        Vector2 pos2 = new Vector2(20, 60);
        Vector2 pos3 = new Vector2(20, 100);
        Rectangle rect = new Rectangle(20, 50, 400, 400);

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
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
            

            if (stopwatch.ElapsedMilliseconds != 0)
            {
                drawTime_avg += stopwatch.ElapsedMilliseconds;
                drawTime_SampleNum--;
                if (drawTime_SampleNum <= 0)
                {
                    drawtime = (int)(drawTime_avg / drawTime_sampleSize);
                    drawTime_SampleNum = drawTime_sampleSize;
                    drawTime_avg = 0;
                }
            }

           
            // Draws stats
            spriteBatch = new SpriteBatch(this.device);
            spriteBatch.Begin();
            
            spriteBatch.DrawString(font, "fps: " + fps.ToString() + " - " + worldMap.GetStats(), pos,Color.White);
            spriteBatch.DrawString(font, "(" + this.PlayerCamera.GetCameraPostion().X + ", " + this.PlayerCamera.GetCameraPostion().Y + ", " + this.PlayerCamera.GetCameraPostion().Z + ")" + " - state " + DebugState() + 
            " drawtime: " + drawtime.ToString() + " ms", pos2, Color.White);


            if (debugImg != null && showDebugImg == true)
            {
                spriteBatch.Draw(debugImg, rect, Color.White);
            }
            spriteBatch.End();

            if (CurrentGameState is GameStates.MousePointerLook || CurrentGameState is GameStates.PlaceBuilding)
            {
                HUD_overlay.Draw(gameTime, this.device);
            }

            base.Draw(gameTime);
        }







        public void ChangeGameState(GameStates.GameState newstate)
        {
            if (CurrentGameState != null)
            {
                CurrentGameState.ExitState();
            }

            if (newstate != null)
            {
                CurrentGameState = newstate;
                CurrentGameState.EnterState();
            }
        }





        private string DebugState() 
        {  
            return CurrentGameState.GetShortName();
        }

    }
}
