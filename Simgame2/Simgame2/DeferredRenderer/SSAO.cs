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

namespace Simgame2.DeferredRenderer
{
    class SSAO
    {
        //SSAO effect
        Effect ssao;

        //SSAO Blur Effect
        Effect ssaoBlur;

        //SSAO Composition effect
        Effect composer;

        //Random Normal Texture
        Texture2D randomNormals;

        //Sample Radius
        float sampleRadius;

        //Distance Scale
        float distanceScale;

        //SSAO Target
        RenderTarget2D SSAOTarget;

        //Blue Target
        RenderTarget2D BlurTarget;

        //FSQ
        FullscreenQuad fsq;

        #region Get Methods

        //Get Sample Radius
        float getSampleRadius() { return sampleRadius; }

        //Get Distance Scale
        float getDistanceScale() { return distanceScale; }

        #endregion

        #region Set Methods

        //Set Sample Radius
        void setSampleRadius(float radius) { this.sampleRadius = radius; }

        //Set Distance Scale
        void setDistanceScale(float scale) { this.distanceScale = scale; }

        #endregion

        //Constructor
        public SSAO(GraphicsDevice GraphicsDevice, ContentManager Content, int Width, int Height)
        {
            //Load SSAO effect
            ssao = Content.Load<Effect>("Effects/SSAO");
            ssao.CurrentTechnique = ssao.Techniques[0];

            //Load SSAO Blur effect
            ssaoBlur = Content.Load<Effect>("Effects/SSAOBlur");
            ssaoBlur.CurrentTechnique = ssaoBlur.Techniques[0];

            //Load SSAO composition effect
            composer = Content.Load<Effect>("Effects/SSAOFinal");
            composer.CurrentTechnique = composer.Techniques[0];

            //Create SSAO Target
            SSAOTarget = new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);

            //Create SSAO Blur Target
            BlurTarget = new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);

            //Create FSQ
            fsq = new FullscreenQuad(GraphicsDevice);

            //Load Random Normal Texture
            randomNormals = Content.Load<Texture2D>("RandomNormals");

            //Set Sample Radius to Default
            sampleRadius = 0;

            //Set Distance Scale to Default
            distanceScale = 0;
        }



        //Draw
        public void Draw(GraphicsDevice GraphicsDevice, RenderTargetBinding[] GBuffer, RenderTarget2D Scene, Camera Camera, RenderTarget2D Output)
        {
            //Set States
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //Render SSAO
            RenderSSAO(GraphicsDevice, GBuffer, Camera);

            //Blur SSAO
            BlurSSAO(GraphicsDevice);

            //Compose final
            Compose(GraphicsDevice, Scene, Output, true);
        }


        //Render SSAO
        void RenderSSAO(GraphicsDevice GraphicsDevice, RenderTargetBinding[] GBuffer, Camera Camera)
        {
            //Set SSAO Target
            GraphicsDevice.SetRenderTarget(SSAOTarget);

            //Clear
            GraphicsDevice.Clear(Color.White);

            //Set Samplers
            GraphicsDevice.Textures[1] = GBuffer[1].RenderTarget;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            GraphicsDevice.Textures[2] = GBuffer[1].RenderTarget;
            GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
            GraphicsDevice.Textures[3] = randomNormals;
            GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;

            //Calculate Frustum Corner of the Camera
            Vector3 cornerFrustum = Vector3.Zero;
            cornerFrustum.Y = (float)Math.Tan(Math.PI / 3.0 / 2.0) * Camera.DrawDistance;
            cornerFrustum.X = cornerFrustum.Y * Camera.AspectRatio;
            cornerFrustum.Z = Camera.DrawDistance;

            //Set SSAO parameters
            ssao.Parameters["Projection"].SetValue(Camera.projectionMatrix);
            ssao.Parameters["cornerFustrum"].SetValue(cornerFrustum);
            ssao.Parameters["sampleRadius"].SetValue(sampleRadius);
            ssao.Parameters["distanceScale"].SetValue(distanceScale);
            ssao.Parameters["GBufferTextureSize"].SetValue(new Vector2(SSAOTarget.Width, SSAOTarget.Height));

            //Apply
            ssao.CurrentTechnique.Passes[0].Apply();

            //Draw
            fsq.Draw(GraphicsDevice);
        }



        //Blur SSAO
        void BlurSSAO(GraphicsDevice GraphicsDevice)
        {
            //Set Blur Target
            GraphicsDevice.SetRenderTarget(BlurTarget);

            //Clear
            GraphicsDevice.Clear(Color.White);

            //Set Samplers, GBuffer was set before so no need to reset...
            GraphicsDevice.Textures[3] = SSAOTarget;
            GraphicsDevice.SamplerStates[3] = SamplerState.LinearClamp;

            //Set SSAO parameters
            ssaoBlur.Parameters["blurDirection"].SetValue(Vector2.One);
            ssaoBlur.Parameters["targetSize"].SetValue(new Vector2(SSAOTarget.Width, SSAOTarget.Height));

            //Apply
            ssaoBlur.CurrentTechnique.Passes[0].Apply();

            //Draw
            fsq.Draw(GraphicsDevice);
        }


        //Compose
        void Compose(GraphicsDevice GraphicsDevice, RenderTarget2D Scene, RenderTarget2D Output, bool useBlurredSSAO)
        {
            //Set Output Target
            GraphicsDevice.SetRenderTarget(Output);

            //Clear
            GraphicsDevice.Clear(Color.White);

            //Set Samplers
            GraphicsDevice.Textures[0] = Scene;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            if (useBlurredSSAO) GraphicsDevice.Textures[1] = BlurTarget;
            else GraphicsDevice.Textures[1] = SSAOTarget;

            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            //Set Effect Parameters
            composer.Parameters["halfPixel"].SetValue(new Vector2(1.0f / SSAOTarget.Width, 1.0f / SSAOTarget.Height));

            //Apply
            composer.CurrentTechnique.Passes[0].Apply();

            //Draw
            fsq.Draw(GraphicsDevice);
        }


        //Modify
        public void Modify(KeyboardState Current)
        {
            float speed = 0.01f;
            if (Current.IsKeyDown(Keys.Z)) sampleRadius -= speed;
            if (Current.IsKeyDown(Keys.X)) sampleRadius += speed;
            if (Current.IsKeyDown(Keys.C)) distanceScale -= speed;
            if (Current.IsKeyDown(Keys.V)) distanceScale += speed;
        }

        //Debug Values
        public void Debug(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            //Width + Height
            int width = 128;
            int height = 128;

            //Set up Drawing Rectangle
            Rectangle rect = new Rectangle(384, 0, width, height);

            //Begin SpriteBatch for Buffer
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null);

            //Draw SSAO buffer
            spriteBatch.Draw((Texture2D)SSAOTarget, rect, Color.White);

            //Draw SSAO Blurred
            rect.X += 128;
            spriteBatch.Draw((Texture2D)BlurTarget, rect, Color.White);

            //End SpriteBatch
            spriteBatch.End();

            //Begin SpriteBatch for Text
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            //Draw sampleRadius
            spriteBatch.DrawString(spriteFont, "Sample Radius: " + sampleRadius.ToString(), new Vector2(0, 128), Color.Red);

            //Draw distanceScale
            spriteBatch.DrawString(spriteFont, "Distance Scale: " + distanceScale.ToString(), new Vector2(0, 148), Color.Blue);

            //End SpriteBatch
            spriteBatch.End();
        }

    }
}
