using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.Buildings
{
    public class Foundation : EntityBuilding
    {

        public Foundation(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.FOUNDATION;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {

            this.AddTexture("Textures/FoundationTex");
            this.scale = scale;
            this.Rotation = rotation;
            this.location = location + StandardTranslation;
            this.LoadModel("Models/Foundation");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            base.Draw(currentViewMatrix, cameraPosition);
            
        }

        public static Vector3 StandardScale = new Vector3(5, 10, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);
        public static Vector3 StandardTranslation = new Vector3(0, 5, 0);




    }






}
