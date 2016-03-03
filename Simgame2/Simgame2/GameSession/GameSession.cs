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


namespace Simgame2.GameSession
{
    public class GameSession : Microsoft.Xna.Framework.GameComponent
    {
        private Game1 game;
        public GameSession(Game game)
            : base(game)
        {
            this.game = (Game1)game;
        }

        public override void Initialize()
        {
            base.Initialize();




        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);



        }



        // Cameras
        public Camera PlayerCamera;


    }
}
