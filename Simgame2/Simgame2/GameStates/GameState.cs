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

namespace Simgame2.GameStates
{
    public class GameState
    {
        public const string Name = "BaseState";

        public GameState(Game1 game)
        {
            this.game = game;
        }

        public virtual void Draw(GameTime gameTime) { }

        public virtual void Update(GameTime gameTime) { }


        protected Game1 game;
    }
}
