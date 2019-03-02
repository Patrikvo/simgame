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
using KiloWatt.Animation.Animation;
using KiloWatt.Animation.Graphics;

namespace Simgame2.Buildings
{
    public class AnimTest : EntityBuilding, KiloWatt.Animation.Graphics.IScene
    {

        public AnimTest(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.TEST;
            this.Isanimated = true;
        }

        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            base.OnEvent(ReceivedEvent);


            return false;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            
            this.AddTexture("test2/lower");  // base texture
            this.AddTexture("test2/upper");  // base texture
            this.AddTexture("test2/head");  // base texture
            this.AddTexture("test2/bow");  // base texture
            this.AddTexture("test2/arrow");  // base texture
            
            /*

            this.scale = scale;
            this.Rotation = rotation;
            this.location = location;
            */
            this.LoadModel("test2/archer");
            /*
            Dictionary<string, object> tag = this.model.Tag as Dictionary<string, object>;
            object aobj = null;
            if (tag != null)
                tag.TryGetValue("AnimationSet", out aobj);
            animations_ = aobj as AnimationSet;

             animInstance = new AnimationInstance(animations_.Animations.ToArray()[1]);
            */

            loadedModel_ = new ModelDraw(this.RunningGameSession.Content.Load<Model>("test2/archer"), "test2/archer");
             loadedModel_.Attach(this);

             //  create a blender that can compose the animations for transition
             blender_ = new AnimationBlender(loadedModel_.Model, loadedModel_.Name);
             loadedModel_.CurrentAnimation = blender_;

             //  remember things about this model
             ResetModelData("test2/archer");

             //  figure out what the animations are, if any.
             LoadAnimations();








            //mediumTransform = this.model.Bones["medium"].Transform;  // Rotor start position
         //   smallTransform = this.model.Bones["small"].Transform;  // Rotor start position
            //rotorTransform = this.model.Bones["Rotor"].Transform;  // Rotor start position
        }



        void ResetModelData(string path)
        {
         //   Heading = 0;
        //    Pitch = 0;
        //    RelativeDistance = 0;
            modelSize_ = loadedModel_.Bounds;
          //  baseDistance_ = (modelSize_.Center.Length() + modelSize_.Radius) * 2;
            modelPath_ = path;
         //   Message = String.Format("Viewing {0}", modelPath_);
            animations_ = null;
        }

        void LoadAnimations()
        {
            //  clear current state
            curAnimationInstance_ = -1;
            instances_ = null;
            blended_ = null;

            //  get the list of animations from our dictionary
            Dictionary<string, object> tag = loadedModel_.Model.Tag as Dictionary<string, object>;
            object aobj = null;
            if (tag != null)
                tag.TryGetValue("AnimationSet", out aobj);
            animations_ = aobj as AnimationSet;

            //  set up animations
            if (animations_ != null)
            {
                instances_ = new AnimationInstance[animations_.NumAnimations];
                //  I'll need a BlendedAnimation per animation, so that I can let the 
                //  blender object transition between them.
                blended_ = new IBlendedAnimation[instances_.Length];
                int ix = 0;
                foreach (Animation a in animations_.Animations)
                {
                    instances_[ix] = new AnimationInstance(a);
                    blended_[ix] = AnimationBlender.CreateBlendedAnimation(instances_[ix]);
                    ++ix;
                }
            }
        }

        IBlendedAnimation GetBlended(int ix)
        {
            unchecked
            {
                return (ix < 0) ? null : (ix >= blended_.Length) ? null : blended_[ix];
            }
        }



     //   AnimationInstance animInstance;
        int curAnimationInstance_ = -1;     //  which animation is playing? (-1 for none)
        AnimationInstance[] instances_;     //  the animation data, as loaded
        IBlendedAnimation[] blended_;       //  state about the different animations (that can change)



        public override void Update(GameTime gameTime)
        {
           // rotorRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);
          //  mediumRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);
         //   smallRotation += (float)((2*rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);


            //      this.model.Bones["Rotor"].Transform = Matrix.CreateRotationZ(rotorRotation) * rotorTransform; 

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt > 0.1f) dt = 0.1f;

            if (blender_ != null)
                blender_.Advance(dt);

           // animInstance.Advance((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (HasMouseFocus)
            {
               // UpdateStatusScreen();
            }
            base.Update(gameTime);
        }

        DrawDetails drawDetails_ = new DrawDetails();

        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {

            
            if (loadedModel_ != null)
            {
                //  the drawdetails set-up can be re-used for all items in the scene
                KiloWatt.Animation.Graphics.DrawDetails dd = drawDetails_;
                dd.dev = this.RunningGameSession.device;// GraphicsDevice;
                dd.fogColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
                dd.fogDistance = 10 * 1000;
                dd.lightAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                dd.lightDiffuse = new Vector4(0.8f, 0.8f, 0.8f, 0);
                dd.lightDir = Vector3.Normalize(new Vector3(1, 3, 2));

                dd.viewInv = Matrix.Invert(currentViewMatrix);
                dd.viewProj = currentViewMatrix * this.RunningGameSession.PlayerCamera.projectionMatrix;// projection_;
                dd.world = Matrix.Identity;

                //  draw the loaded model (the only model I have)
                loadedModel_.ScenePrepare(dd);
                if (loadedModel_.SceneDraw(dd, 0))
                {
                    loadedModel_.SceneDrawTransparent(dd, 0);
                }
            }
           
    

      
            //base.Draw(currentViewMatrix, cameraPosition);

        }

        public static Vector3 StandardScale = new Vector3(100, 100, 100);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);
        private AnimationBlender blender_;
        private BoundingSphere modelSize_;
        private string modelPath_;



    //    private Matrix mediumTransform;
    //    private Matrix smallTransform;

    //    private float mediumRotation = 0;
    //    private float smallRotation = 0;
    //    private float rotationSpeed = 1;



        public AnimationSet animations_ { get; set; }

        public ModelDraw loadedModel_ { get; set; }

        public int TechniqueIndex(string technique)
        {
            return 0;
        }

        public void AddRenderable(ISceneRenderable sr)
        {
            throw new NotImplementedException();
        }

        public void RemoveRenderable(ISceneRenderable sr)
        {
            throw new NotImplementedException();
        }

        public ISceneTexture GetSceneTexture()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
