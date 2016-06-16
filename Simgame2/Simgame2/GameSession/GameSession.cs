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
using System.IO;


namespace Simgame2.GameSession
{
    public class GameSession
    {
        public const int mapNumCellsPerSide = 641; // 20 40 80 160 320 640 

        
        
        public GameSession(Game game)
        {
            this.game = (Game1)game;
        }


    


        public void Initialize(GraphicsDevice graphicsDevice)
        {

            this.device = graphicsDevice;

            this.HUD_overlay = new GUI(this);

            this.simulator = new Simulation.Simulator(this);
            this.simulator.AddEntity(this.HUD_overlay);


            this.FreeLookState = new GameStates.FreeLook(this);
            this.MousePointerLookState = new GameStates.MousePointerLook(this);
            this.PlaceBuildingState = new GameStates.PlaceBuilding(this);
            this.debugState = new GameStates.DebugState(this);

            this.AvailableResources = new Simulation.ResourceStorage();
        }

        public void LoadContent(ContentManager Content)
        {
            this.Content = Content;

            this.effect = Content.Load<Effect>("Series4Effects");

            this.font = Content.Load<SpriteFont>("Courier New");

            this.PlayerCamera = new Camera(this.device.Viewport.AspectRatio);
            
            this.PlayerCamera.DoFixedAltitude = true;  

            this.LODMap = new LODTerrain.LODTerrain(this, GameSession.mapNumCellsPerSide, GameSession.mapNumCellsPerSide, this.effect, this.device, null);

            this.PlayerCamera.LODMap = this.LODMap;

            this.simulator.AddEntity(this.LODMap);
            this.simulator.AddEntity(this.PlayerCamera);

            this.PlayerCamera.DrawDistance = 1200.0f; // 300.0f;

            this.HUD_overlay.PreloadImages(Content);

            this.HUD_overlay.ConstructButtons();

            this.textureGenerator = new TextureGenerator(this);

            this.LODMap.GetRenderer().waterBumpMap = Content.Load<Texture2D>("waterbump");

            this.LODMap.GetRenderer().Textures = new Texture2D[5];
            this.LODMap.GetRenderer().Textures[0] = Content.Load<Texture2D>("Textures\\tex1");
            this.LODMap.GetRenderer().Textures[1] = Content.Load<Texture2D>("Textures\\tex0");
            this.LODMap.GetRenderer().Textures[2] = Content.Load<Texture2D>("Textures\\tex2");
            this.LODMap.GetRenderer().Textures[3] = Content.Load<Texture2D>("Textures\\tex3");
            this.LODMap.GetRenderer().Textures[4] = Content.Load<Texture2D>("Textures\\tex1");

            this.LODMap.selectionTexture = this.textureGenerator.SelectionImage(Color.Yellow, 5); //WorldMap.mapCellScale);

            // skydome
            this.LODMap.GetRenderer().LoadSkyDome(Content.Load<Model>("Models/dome"));


            this.PlayerCamera.UpdateViewMatrix();


            this.entityFactory = EntityFactory.CreateFactory(this, this.LODMap, this.PlayerCamera.projectionMatrix);
        }

        public void LoadContent(ContentManager Content, GameStorage storage, GameStorage mapstorage)
        {
            this.Content = Content;

            this.effect = Content.Load<Effect>("Series4Effects");

            this.font = Content.Load<SpriteFont>("Courier New");

            this.PlayerCamera = new Camera(this.device.Viewport.AspectRatio);

            this.PlayerCamera.DoFixedAltitude = true;

            this.PlayerCamera.Restore(storage);
            this.LODMap = new LODTerrain.LODTerrain(this, GameSession.mapNumCellsPerSide, GameSession.mapNumCellsPerSide, this.effect, this.device, mapstorage);

            this.PlayerCamera.LODMap = this.LODMap;

            this.PlayerCamera.DrawDistance = 1200.0f; // 300.0f;

            this.HUD_overlay.PreloadImages(Content);

            this.HUD_overlay.ConstructButtons();

            this.textureGenerator = new TextureGenerator(this);

            this.LODMap.GetRenderer().waterBumpMap = Content.Load<Texture2D>("waterbump");

            this.LODMap.GetRenderer().Textures = new Texture2D[5];
            this.LODMap.GetRenderer().Textures[0] = Content.Load<Texture2D>("Textures\\tex1");
            this.LODMap.GetRenderer().Textures[1] = Content.Load<Texture2D>("Textures\\tex0");
            this.LODMap.GetRenderer().Textures[2] = Content.Load<Texture2D>("Textures\\tex2");
            this.LODMap.GetRenderer().Textures[3] = Content.Load<Texture2D>("Textures\\tex3");
            this.LODMap.GetRenderer().Textures[4] = Content.Load<Texture2D>("Textures\\tex1");
     //       this.LODMap.GetRenderer().WaterTexture = Content.Load<Texture2D>("Textures\\water");

            this.LODMap.selectionTexture = this.textureGenerator.SelectionImage(Color.Yellow, 5); //WorldMap.mapCellScale);

            // skydome
            this.LODMap.GetRenderer().LoadSkyDome(Content.Load<Model>("Models/dome"));


            this.PlayerCamera.UpdateViewMatrix();


            this.entityFactory = EntityFactory.CreateFactory(this, this.LODMap, this.PlayerCamera.projectionMatrix);
        }

        public void Update(GameTime gameTime)
        {
            this.simulator.Update(gameTime);
            this.CurrentGameState.Update(gameTime);
        }


        public void Draw(GameTime gameTime)
        {
            this.device.Clear(Color.Black);
            this.device.DepthStencilState = DepthStencilState.Default;   // fixes the Z-buffer. 


            this.LODMap.Draw(this.PlayerCamera, gameTime);


            if (this.CurrentGameState is GameStates.MousePointerLook || this.CurrentGameState is GameStates.PlaceBuilding)
            {
                this.HUD_overlay.Draw(gameTime, this.device);
            }

        }

       


        public void ChangeGameState(GameStates.GameState newstate)
        {
            if (this.CurrentGameState != null)
            {
                this.CurrentGameState.ExitState();
            }

            if (newstate != null)
            {
                this.CurrentGameState = newstate;
                this.CurrentGameState.EnterState();
            }
        }


        public Exception SaveGameWriteExeption { get; set; }
        public bool SaveGame(string filename)
        {
            bool result = false;

            GameStorage storage = new GameStorage(filename, true);


            try
            {
                this.PlayerCamera.Store(storage);
             //   this.LODMap.Store(storage);


                    
                    // this.entityFactory
                    // this.CurrentGameState


                result = true;
                storage.Close();
            }
            catch(Exception ex)
            {
                SaveGameWriteExeption = ex;
                Console.WriteLine("Savegame exception occured: ");
                Console.WriteLine(ex.ToString());
            }
            return result;
        }


        public Exception LoadGameReadExection { get; set; }
        public bool LoadGame(string filename)
        {
            bool result = false;

            if (File.Exists(filename))
            {
                GameStorage storage = new GameStorage(filename, false);
                GameStorage mapStorage = new GameStorage("map.dat", false);

                try
                {
                    this.PlayerCamera.Restore(storage);
                    this.LODMap = new LODTerrain.LODTerrain(this, GameSession.mapNumCellsPerSide, GameSession.mapNumCellsPerSide, this.effect, this.device, mapStorage);
                    result = true;
                }
                catch (Exception ex)
                {
                    LoadGameReadExection = ex;
                    Console.WriteLine("Loadgame exception occured: ");
                    Console.WriteLine(ex.ToString());
                }

            }




            return result;

        }





        public Game1 game;

        public ContentManager Content;


        // Cameras
        public Camera PlayerCamera;



        // Rendering objects
        public GraphicsDevice device;
        public Effect effect;


        // Game Objects
        public LODTerrain.LODTerrain LODMap;
        public TextureGenerator textureGenerator;

        public GameStates.GameState CurrentGameState;
        public GameStates.FreeLook FreeLookState;
        public GameStates.MousePointerLook MousePointerLookState;
        public GameStates.PlaceBuilding PlaceBuildingState;
        public GameStates.DebugState debugState;

        public Simulation.Simulator simulator;

        public Simulation.ResourceStorage AvailableResources;

        public EntityFactory entityFactory;

        public EntityBuilding selBuilding;

        // GUI

        public GUI HUD_overlay;

        // Input/output objects
        public SpriteFont font;

        

        // Debug

        public bool showDebugImg = false;
    }
}
