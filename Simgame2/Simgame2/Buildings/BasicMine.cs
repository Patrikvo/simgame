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

namespace Simgame2.Buildings
{
    public class BasicMine: EntityBuilding
    {
        public BasicMine(Game game)
            : base(game)
        {
            this.projectionMatrix = ((Game1)game).projectionMatrix;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.LoadModel("BasicMine", effect);
            this.AddTexture(this.Game.Content.Load<Texture2D>("BasicMineTex"));
            this.scale = scale;
            this.rotation = rotation;
            this.location = location;
        }


        public void Place(WorldMap map, Vector3 location, bool flatten)
        {
            this.location = location;
            this.PlaceBuilding(map, true); 
        }

        

        public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

    }
}
