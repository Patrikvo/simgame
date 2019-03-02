using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simgame2.Entities
{
    public class Animation
    {
        public Animation(Model model, string[] boneNames)
        {
            this.model = model;
            this.boneNames = boneNames;

            this.InitialBoneTransforms = new Matrix[this.boneNames.Length];
            for (int i = 0; i < this.boneNames.Length; i++)
            {
                this.InitialBoneTransforms[i] = this.model.Bones[this.boneNames[i]].Transform;
            }
            HasFinished = false;
            NextAnimation = null;
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Reset()
        {
            this.HasFinished = false;
            
        }

        public void ResetModelTransforms()
        {
            for (int i = 0; i < this.boneNames.Length; i++)
            {
                this.model.Bones[this.boneNames[i]].Transform = this.InitialBoneTransforms[i];
            }
        }

        public virtual void SetModelTransforms()
        {

        }


        public bool HasFinished { get; protected set; }

        public Animation NextAnimation;
    


        protected Model model;
        protected string[] boneNames;

        protected Matrix[] InitialBoneTransforms;


    }
}
