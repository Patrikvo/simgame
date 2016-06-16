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

namespace Simgame2.Entities
{
    public class EntityFlyer : Entity
    {
        public EntityFlyer(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.MaxSpeed = 10.0f;
            this.Velocity = new Vector3(0.0f, 0.0f, -1.0f);

            this.AddTexture("CubeTex");
            this.LoadModel("Models/Cube"); //, this.effect);
            CorrectBoundingBox = true;
            this.Type = EntityTypes.FLYER;
            this.CanBeCommanded = false;
            this.IsMover = true;
        }


        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            base.OnEvent(ReceivedEvent);
            // TODO IMPLEMENT
            return false;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateDirection(gameTime);
            this.location = this.location + (this.Velocity * (this.MaxSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000));
            this.UpdateBoundingBox();

            

            
            // TODO set location, update distace, check if distance < minDistance, then change target
            if (this.HasMouseFocus)
            {
                Console.WriteLine("Flyer");
            }

        }



        // this.location = this.location + (this.Velocity * (this.MaxSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000));

        public void UpdateDirection(GameTime gameTime)
        {
            // TODO create nice flightpath


            float altitude = this.location.Y;
            float VerticalDirection = this.Velocity.Y;
            if (altitude > MinHeight + ((MaxHeight - MinHeight) / 2))
            {
                VerticalDirection = VerticalDirection - (0.1f * gameTime.ElapsedGameTime.Milliseconds / 1000);
            }
            else if (altitude < MinHeight - ((MaxHeight - MinHeight) / 2))
            {
                VerticalDirection = VerticalDirection + (0.1f * gameTime.ElapsedGameTime.Milliseconds / 1000);
            }

            double x = (this.Velocity.X + (0.1f * gameTime.ElapsedGameTime.Milliseconds / 1000));
            if (x > (2 * Math.PI)) { x = x - (2* Math.PI); }
            double y = (this.Velocity.Y + (0.1f * gameTime.ElapsedGameTime.Milliseconds / 1000));
            if (y > (2 * Math.PI)) { y = y - (2 * Math.PI); }
                

            this.Velocity = new Vector3((float)x, VerticalDirection, (float)y);
            this.Velocity.Normalize();






        }

        public float MinHeight = 50.0f;
        public float MaxHeight = 100.0f;



        public bool CollidesWith(Entity other)
        {
            if (!this.boundingBox.Equals(other.boundingBox))
            {
                if (boundingBox.Intersects(other.boundingBox) || boundingBox.Contains(other.boundingBox) == ContainmentType.Contains || other.boundingBox.Contains(boundingBox) == ContainmentType.Contains)
                {
                    return true;
                }
            }
            return false;
        }



        public float MaxSpeed { get; set; }
        public Vector3 Velocity { get; set; }
    }
}
