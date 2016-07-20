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
    

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GameSession.GameSession RunningGameSession;


        // Rendering objects
        private GraphicsDeviceManager graphics;

        private bool doneLoading = false;




        //AnimationPlayer animationPlayer;
        

        public Game1()
        {
            this.IsFixedTimeStep = false;    // fixes stutter problem in Win 10.
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.RunningGameSession = new GameSession.GameSession(this);
        }


        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 900;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            this.RunningGameSession.Initialize(graphics.GraphicsDevice);
            base.Initialize();
        }




        protected override void LoadContent()
        {
            if (System.IO.File.Exists("savegame.dat") && System.IO.File.Exists("map.dat"))
            {
                GameSession.GameStorage storage = new GameSession.GameStorage("savegame.dat", false);
                GameSession.GameStorage mapstorage = new GameSession.GameStorage("map.dat", false);
                this.RunningGameSession.LoadContent(this.Content, storage, mapstorage);

            }
            else
            {
                this.RunningGameSession.LoadContent(this.Content);
            }
            

            doneLoading = true;
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.FreeLookState);
        }



        protected override void UnloadContent()
        {
        }


        

        protected override void Update(GameTime gameTime)
        {
            stopwatch.Reset();
            stopwatch.Start();

            RunningGameSession.Update(gameTime);

            base.Update(gameTime);
            stopwatch.Stop();
        }




        protected override void Draw(GameTime gameTime)
        {
            if (!doneLoading) { return; }


            MeasureFramerate(gameTime);

            RunningGameSession.Draw(gameTime);


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
            spriteBatch = new SpriteBatch(this.RunningGameSession.device);
            spriteBatch.Begin();

            spriteBatch.DrawString(this.RunningGameSession.font, "fps: " + fps.ToString() + " - " + this.RunningGameSession.LODMap.GetStats(), pos, Color.White);
            spriteBatch.DrawString(this.RunningGameSession.font, "(" + this.RunningGameSession.PlayerCamera.GetCameraPostion().X + ", " + this.RunningGameSession.PlayerCamera.GetCameraPostion().Y + ", " + this.RunningGameSession.PlayerCamera.GetCameraPostion().Z + ")" + " - state " + DebugState() + 
            " drawtime: " + drawtime.ToString() + " ms", pos2, Color.White);


            if (debugImg != null && this.RunningGameSession.showDebugImg == true)
            {
                spriteBatch.Draw(debugImg, rect, Color.White);
            }
            spriteBatch.End();



            



            base.Draw(gameTime);
        }

        private void MeasureFramerate(GameTime gameTime)
        {
            // measure frame rate
            if (gameTime.ElapsedGameTime.Milliseconds != 0)
            {
                fps_avg += (1000 / gameTime.ElapsedGameTime.Milliseconds);
                fps_SampleNum--;
                if (fps_SampleNum <= 0)
                {
                    fps = (int)(fps_avg / fps_sampleSize);
                    fps_SampleNum = fps_sampleSize;
                    fps_avg = 0;
                }

            }
        }













        private string DebugState() 
        {
            return RunningGameSession.CurrentGameState.GetShortName();
        }



        public Texture2D debugImg = null;


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

    }
}
