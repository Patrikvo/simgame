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
using Simgame2.Animation;
using AnimationAux;

namespace Simgame2.Entities
{
    public class AnimTest : EntityBuilding
    {
        public AnimTest(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.model = new AnimatedModel("test2/Victoria-hat-tpose");
            this.model.LoadContent(this.RunningGameSession.Content);

            base.model = this.model.model;

            dance = new AnimatedModel("test2/Victoria-hat-dance");
            dance.LoadContent(this.RunningGameSession.Content);

            // Obtain the clip we want to play. I'm using an absolute index, 
            // because XNA 4.0 won't allow you to have more than one animation
            // associated with a model, anyway. It would be easy to add code
            // to look up the clip by name and to index it by name in the model.
            AnimationClip clip = dance.Clips[0];


            // And play the clip
            AnimationPlayer player = model.PlayClip(clip);
            player.Looping = true;


        //    this.Rotation = new Vector3(0.0f, 0.0f, (float)(Math.PI/2));


        }


        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            base.OnEvent(ReceivedEvent);
            // TODO IMPLEMENT
            return false;
        }


        public override void Update(GameTime gameTime)
        {
            this.model.Update(gameTime);
            base.Update(gameTime);
        }


        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            this.model.Draw(device, playerCamera, this.GetWorldMatrix());
        }



        private AnimatedModel model = null;
        private AnimatedModel dance = null;

    }
}
