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
    public class WindTower : EntityBuilding
    {
        public WindTower(Game game)
            : base(game)
        {
            
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.LoadModel("WindTower", effect);

            this.AddTexture(this.Game.Content.Load<Texture2D>("WindTower_bTex"));  // base texture
            this.AddTexture(this.Game.Content.Load<Texture2D>("WindTower_rTex"));  // rotor texture
          

            this.scale = scale;
            this.rotation = rotation;
            this.location = location;

            rotorTransform = this.model.Bones["Rotor"].Transform;  // Rotor start position
        }

        public override void Update(GameTime gameTime)
        {
            rotorRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2*Math.PI);
            
            base.Update(gameTime);
        }

        public void Place(WorldMap map, Vector3 location, bool flatten)
        {
            this.location = location;
            this.PlaceBuilding(map, true);
        }



        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            Matrix worldMatrix = Matrix.CreateScale(scale) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y) * 
                Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(location);

            this.model.Bones["Rotor"].Transform = Matrix.CreateRotationZ(rotorRotation) * rotorTransform; 
                

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);


            int meshNum = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                
                foreach (Effect currentEffect in mesh.Effects)
                {
                    
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];

                    currentEffect.Parameters["xTexture"].SetValue(this.texture[meshNum]);
                    currentEffect.Parameters["xWorld"].SetValue(transforms[mesh.ParentBone.Index] * worldMatrix);


                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xAmbient"].SetValue(0.4f);
                    currentEffect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

                    currentEffect.Parameters["cameraPos"].SetValue(cameraPosition);
                    currentEffect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
                    currentEffect.Parameters["FogNear"].SetValue(FOGNEAR);
                    currentEffect.Parameters["FogFar"].SetValue(FOGFAR);


                }
                mesh.Draw();
                meshNum++;
            }
        }


        public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

        private float rotorRotation = 0;
        private float rotationSpeed = 1;
        private Matrix rotorTransform;
    }
}
