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
    class PointLight
    {

        //Position
        Vector3 position;

        //Radius
        float radius;

        //Color
        Vector4 color;

        //Intensity
        float intensity;

        //ShadowMap
        RenderTargetCube shadowMap;

        //Is this Light with Shadows?
        bool isWithShadows;

        //Shadow Map Resoloution
        int shadowMapResoloution;

        #region Get Functions

        //Get Position
        public Vector3 getPosition() { return position; }

        //Get Radius
        public float getRadius() { return radius; }

        //Get Color
        public Vector4 getColor() { return color; }

        //Get Intensity
        public float getIntensity() { return intensity; }

        //Get IsWithShadows
        public bool getIsWithShadows() { return isWithShadows; }

        //Get ShadowMapResoloution
        public int getShadowMapResoloution()
        {
            if (shadowMapResoloution < 2048) return shadowMapResoloution;
            else return 2048;
        }

        //Get DepthBias
        public float getDepthBias() { return (1.0f / (20 * radius)); }

        //Get ShadowMap
        public RenderTargetCube getShadowMap() { return shadowMap; }

        #endregion

        #region Set Functions

        //Set Position
        public void setPosition(Vector3 position) { this.position = position; }

        //Set Radius
        public void setRadius(float radius) { this.radius = radius; }

        //Set Color
        public void setColor(Color color) { this.color = color.ToVector4(); }

        //Set Color
        public void setColor(Vector4 color) { this.color = color; }

        //Set Intensity
        public void setIntensity(float intensity) { this.intensity = intensity; }

        //Set isWithShadows
        public void setIsWithShadows(bool shadows) { this.isWithShadows = shadows; }

        #endregion

        //Constructor
        public PointLight(GraphicsDevice GraphicsDevice, Vector3 Position, float Radius, Vector4 Color, float Intensity, bool isWithShadows, int shadowMapResoloution)
        {
            //Set Position
            setPosition(Position);

            //Set Radius
            setRadius(Radius);

            //Set Color
            setColor(Color);

            //Set Intensity
            setIntensity(Intensity);

            //Set isWithShadows
            this.isWithShadows = isWithShadows;

            //Set shadowMapResoloution
            this.shadowMapResoloution = shadowMapResoloution;

            //Make ShadowMap
            shadowMap = new RenderTargetCube(GraphicsDevice, getShadowMapResoloution(), false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
        }

        //Create World Matrix for Deferred Rendering Geometry
        public Matrix World()
        {
            //Make Scaling Matrix
            Matrix scale = Matrix.CreateScale(radius / 100.0f);

            //Make Translation Matrix
            Matrix translation = Matrix.CreateTranslation(position);

            //Return World Transform
            return (scale * translation);
        }

    }
}
