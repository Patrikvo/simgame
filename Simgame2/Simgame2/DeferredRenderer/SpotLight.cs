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
    public class SpotLight
    {

        //Position
        Vector3 position;

        //Direction
        Vector3 direction;

        //Color
        Vector4 color;

        //Intensity
        float intensity;

        //NearPlane
        float nearPlane;

        //FarPlane
        float farPlane;

        //FOV
        float FOV;

        //Is this Light with Shadows?
        bool isWithShadows;

        //Shadow Map Resoloution
        int shadowMapResoloution;

        //DepthBias for the Shadowing... (1.0f / 2000.0f)
        float depthBias;

        //World(for geometry in LightMapping phase...)
        Matrix world;

        //View
        Matrix view;

        //Projection
        Matrix projection;

        //Shadow Map
        RenderTarget2D shadowMap;

        //Attenuation Texture
        Texture2D attenuationTexture;

        #region Get Functions

        //Get Position
        public Vector3 getPosition() { return position; }

        //Get Direction
        public Vector3 getDirection() { return direction; }

        //Get Color
        public Vector4 getColor() { return color; }

        //Get Intensity
        public float getIntensity() { return intensity; }

        //Get NearPlane
        public float getNearPlane() { return nearPlane; }

        //Get FarPlane
        public float getFarPlane() { return farPlane; }

        //Get FOV
        public float getFOV() { return FOV; }

        //Get IsWithShadows
        public bool getIsWithShadows() { return isWithShadows; }

        //Get ShadowMapResoloution
        public int getShadowMapResoloution()
        {
            if (shadowMapResoloution < 2048)
                return shadowMapResoloution;
            else
                return 2048;
        }

        //Get DepthBias
        public float getDepthBias() { return depthBias; }

        //Get World
        public Matrix getWorld() { return world; }

        //Get View
        public Matrix getView() { return view; }

        //Get Projection
        public Matrix getProjection() { return projection; }

        //Get ShadowMap
        public RenderTarget2D getShadowMap() { return shadowMap; }

        //Get Attenuation Texture
        public Texture2D getAttenuationTexture() { return attenuationTexture; }

        #endregion

        #region Set Functions

        //Set Position
        public void setPosition(Vector3 position) { this.position = position; }

        //Set Direction
        public void setDirection(Vector3 direction) { direction.Normalize(); this.direction = direction; }

        //Set Color
        public void setColor(Vector4 color) { this.color = color; }

        //Set Color
        public void setColor(Color color) { this.color = color.ToVector4(); }

        //Set Intensity
        public void setIntensity(float intensity) { this.intensity = intensity; }

        //Set isWithShadows
        public void setIsWithShadows(bool iswith) { this.isWithShadows = iswith; }

        //Set DepthBias
        public void setDepthBias(float bias) { this.depthBias = bias; }

        //Set Attenuation Texture
        public void setAttenuationTexture(Texture2D attenuationTexture) { this.attenuationTexture = attenuationTexture; }

        #endregion

        //Constructor
        public SpotLight(GraphicsDevice GraphicsDevice, Vector3 Position, Vector3 Direction, Vector4 Color, float Intensity,
        bool isWithShadows, int ShadowMapResoloution, Texture2D AttenuationTexture)
        {
            //Position
            setPosition(Position);

            //Direction
            setDirection(Direction);

            //Color
            setColor(Color);

            //Intensity
            setIntensity(Intensity);

            //NearPlane
            nearPlane = 1.0f;

            //FarPlane
            farPlane = 100.0f;

            //FOV
            FOV = MathHelper.PiOver2;

            //Set whether Is With Shadows
            setIsWithShadows(isWithShadows);

            //Shadow Map Resoloution
            shadowMapResoloution = ShadowMapResoloution;

            //Depth Bias
            depthBias = 1.0f / 2000.0f;

            //Projection
            projection = Matrix.CreatePerspectiveFieldOfView(FOV, 1.0f, nearPlane, farPlane);

            //Shadow Map
            shadowMap = new RenderTarget2D(GraphicsDevice, getShadowMapResoloution(), getShadowMapResoloution(), false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            //Attenuation Texture
            attenuationTexture = AttenuationTexture;

            //Create View and World
            Update();
        }

        //Calculate the Cosine of the LightAngle
        public float LightAngleCos()
        {
            //float ConeAngle = 2 * atanf(Radius / Height);
            float ConeAngle = FOV;
            return (float)Math.Cos((double)ConeAngle);
        }

        //Update
        public void Update()
        {
            //Target
            Vector3 target = (position + direction);
            if (target == Vector3.Zero) target = -Vector3.Up;

            //Up
            Vector3 up = Vector3.Cross(direction, Vector3.Up);
            if (up == Vector3.Zero) up = Vector3.Right;
            else up = Vector3.Up;

            //ReMake View
            view = Matrix.CreateLookAt(position, target, up);

            //Make Scaling Factor
            float radial = (float)Math.Tan((double)FOV / 2.0) * 2 * farPlane;

            //Make Scaling Matrix
            Matrix Scaling = Matrix.CreateScale(radial, radial, farPlane);

            //Make Translation Matrix
            Matrix Translation = Matrix.CreateTranslation(position.X, position.Y, position.Z);

            //Make Inverse View
            Matrix inverseView = Matrix.Invert(view);

            //Make Semi-Product
            Matrix semiProduct = Scaling * inverseView;

            //Decompose Semi-Product
            Vector3 S; Vector3 P; Quaternion Q;
            semiProduct.Decompose(out S, out Q, out P);

            //Make Rotation
            Matrix Rotation = Matrix.CreateFromQuaternion(Q);

            //Make World
            world = Scaling * Rotation * Translation;

        }
    }
}
