using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Simgame2
{
    public abstract class Entity: Simulation.Events.EventReceiver
    {

        public enum EntityTypes { NONE, FOUNDATION, BASIC_MINE, MELTER, SOLAR, WIND_TOWER, MOVER, FLYER, LANDER };

        

        protected GameSession.GameSession RunningGameSession;

        public enum States { UNDER_CONSTRUCTION, IDLE, ACTIVE, UPGRADING, BEING_REMOVED }
        private States _CurrentState;

        public States CurrentState
        {
            get { return _CurrentState; }
            set { _CurrentState = value; }
        }



        public Entity(GameSession.GameSession RunningGameSession)
        {
            this.RunningGameSession = RunningGameSession;
            basicEffect = new BasicEffect(this.RunningGameSession.device);
            
            this.Type = EntityTypes.NONE;
            this.IsGhost = false;
            this.CanBeCommanded = false;
            this.IsMover = false;
            
        }


        public bool CanBeCommanded { get; set; }
        public bool IsMover { get; set; }

        public bool IsVisible { get; set; }

        
        public bool HideBillboard { get; set; }

        public LODTerrain.LODTerrain LODMap { get { return this.RunningGameSession.LODMap; } }

        public EntityFactory entityFactory { get { return this.RunningGameSession.entityFactory; } }
        public GraphicsDevice device { get { return RunningGameSession.device; } }
        public Effect effect { get { return this.RunningGameSession.effect; } }
        public SpriteFont font { get { return this.RunningGameSession.font; } }
        public Camera playerCamera { get { return this.RunningGameSession.PlayerCamera; } }

        public ResourceCell getResourceCell(float wx, float wy )
        {
            return this.LODMap.GetResourceFromWorldCoor(wx, wy);
        }


        public virtual bool OnEvent(Simulation.Event ReceivedEvent)
        {
            if (ReceivedEvent is Simulation.Events.BuildingContructionDoneEvent)
            {
                Simulation.Events.BuildingContructionDoneEvent e = (Simulation.Events.BuildingContructionDoneEvent)ReceivedEvent;

                Console.WriteLine("Saw New Building");

                return true;
            }

            if (ReceivedEvent is Simulation.Events.EntityLostFocusEvent)
            {
                Simulation.Events.EntityLostFocusEvent e = (Simulation.Events.EntityLostFocusEvent)ReceivedEvent;

                if (e.SourceEntity == this)
                {
                    this.HasMouseFocus = false;
                }

            }

            if (ReceivedEvent is Simulation.Events.EntityHasFocusEvent)
            {
                Simulation.Events.EntityHasFocusEvent e = (Simulation.Events.EntityHasFocusEvent)ReceivedEvent;

                if (e.SourceEntity == this)
                {
                    this.HasMouseFocus = true;
                }

            }

            return false;
        }


        public Entity(Entity other)
        {
            this.HideBillboard = false;
            this.model = other.model;
            this.location = new Vector3(other.location.X, other.location.Y, other.location.Z);
            this.Rotation = new Vector3(other.Rotation.X, other.Rotation.Y, other.Rotation.Z);
            this.scale = new Vector3(other.scale.X, other.scale.Y, other.scale.Z);

            this.texture = other.texture;
            this.texture = new Texture2D[other.texture.Length];
            for (int i = 0; i < other.texture.Length; i++)
            {
                this.texture[i] = other.texture[i];
            }

            this.projectionMatrix = other.projectionMatrix;

            this.boundingBox = UpdateBoundingBox();
            generateTags();  // prelighting
        }

        public EntityTypes Type;

   
        public virtual void Initialize()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
        }


        public float DistanceTo(Entity other)
        {
            float distance = float.MaxValue;
            if (other != null)
            {
                distance = Vector3.Distance(other.location, this.Location);
            }
            return distance;
        }


        public void LoadModel(string assetName) //, Effect effect)
        {
            model = this.RunningGameSession.Content.Load<Model>(assetName); 
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            this.boundingBox = UpdateBoundingBox();
            generateTags();  // prelighting
        }


        // prelighting -->

        // Sets the specified effect parameter to the given effect, if it
        // has that parameter
        void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
        }

        public void SetModelEffect(Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;

                    // Copy the effect if necessary
                    if (CopyEffect)
                        toSet = effect.Clone();

               /*     MeshTag tag = ((MeshTag)part.Tag);

                    // If this ModelMeshPart has a texture, set it to the effect
                    if (tag.Texture != null)
                    {
                        setEffectParameter(toSet, "BasicTexture", tag.Texture);
                        setEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                        setEffectParameter(toSet, "TextureEnabled", false);

                    // Set our remaining parameters to the effect
                    setEffectParameter(toSet, "DiffuseColor", tag.Color);
                    setEffectParameter(toSet, "SpecularPower", tag.SpecularPower);
                    */
                    part.Effect = toSet;
                }
        }

        // Store references to all of the model's current effects
        public void CacheEffects()
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    ((MeshTag)part.Tag).CachedEffect = part.Effect;
        }

        // Restore the effects referenced by the model's cache
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag)part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag)part.Tag).CachedEffect;
        }

        private void generateTags()
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor,
                            effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }

        // < ---    // prelighting



        public void AddTexture(string textureName)
        {
            AddTexture(this.RunningGameSession.Content.Load<Texture2D>(textureName));
        }


        public void AddTexture(Texture2D tex)
        {
            if (this.texture == null)  // first texture to add
            {
                this.texture = new Texture2D[1];
                this.texture[0] = tex;
            }
            else   // make new larger array, add texture, replace original texture array.
            {
                Texture2D[] textures = new Texture2D[this.texture.Length + 1];
                for (int i = 0; i < this.texture.Length; i++)
                {
                    textures[i] = this.texture[i];
                }


                textures[this.texture.Length] = tex;

                this.texture = textures;
            }
        }

        public float FOGNEAR;
        public float FOGFAR;
        public Color FOGCOLOR;


        public Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(scale) * Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) *
                    Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateTranslation(location);
        }

        public Matrix[] GetBoneTransforms()
        {
            Matrix[] xwingTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(xwingTransforms);
            return xwingTransforms;
        }

        public virtual void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            
            this.Draw(currentViewMatrix, this.projectionMatrix, cameraPosition);
        }

        public virtual void Draw(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {

            Matrix worldMatrix = GetWorldMatrix();
            LODMap.GetRenderer().DrawModel(this.model, this.texture, this.IsTransparant, this.CanPlace, this.HasMouseFocus, this.GetWorldMatrix(), currentViewMatrix,
                projectionMatrix, cameraPosition, "Textured");

            if (ShowBoundingBox)
            {
                DrawBoundingBox(currentViewMatrix, cameraPosition);
            }
        }

        public virtual void DrawShadow(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            Matrix worldMatrix = GetWorldMatrix();
            LODMap.GetRenderer().DrawShadow(this.model, this.GetWorldMatrix(), currentViewMatrix,projectionMatrix, "Textured");
        }



        BasicEffect basicEffect;
        short[] bBoxIndices = {
                    0, 1, 1, 2, 2, 3, 3, 0, // Front edges
                    4, 5, 5, 6, 6, 7, 7, 4, // Back edges
                    0, 4, 1, 5, 2, 6, 3, 7 // Side edges connecting front and back
                    };

        private VertexPositionColor[] BoundingBoxprimitiveList;
        public void DrawBoundingBox(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            
            basicEffect.World = Matrix.Identity;
            basicEffect.View = currentViewMatrix;
            basicEffect.Projection = projectionMatrix;
            

            basicEffect.TextureEnabled = false;

            // Draw the box with a LineList
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                RunningGameSession.device.DrawUserIndexedPrimitives(PrimitiveType.LineList, BoundingBoxprimitiveList, 0, 8, bBoxIndices, 0, 12);
            }
        }


        public bool CorrectBoundingBox { get; set; }
        public BoundingBox UpdateBoundingBox()
        {

            Matrix worldTransform = Matrix.CreateScale(scale) * Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateTranslation(location);

            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

       //     worldTransform *= Matrix.CreateTranslation(0.0f, (model.Meshes[0].BoundingSphere.Radius * scale.Y)/2, 0.0f);
            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            if (CorrectBoundingBox == true)
            {
                float tranlationCorrection = (max.Y - min.Y) / 2;
                min = new Vector3(min.X, min.Y + tranlationCorrection, min.Z);
                max = new Vector3(max.X, max.Y + tranlationCorrection, max.Z);
            }
            // Create and return bounding box
            boundingBox = new BoundingBox(min, max);
            corners = boundingBox.GetCorners();




                BoundingBoxprimitiveList = new VertexPositionColor[this.corners.Length];

            // Assign the 8 box vertices
            for (int i = 0; i < this.corners.Length; i++)
            {
                BoundingBoxprimitiveList[i] = new VertexPositionColor(this.corners[i], Color.White);
            }
            return boundingBox;
        }

        public bool HasMouseFocus { get; set; }


        public float getAltitude()
        {
            float altitude = int.MaxValue;
            foreach (Vector3 v in corners)
            {
                if (v.Y < altitude) { altitude = v.Y; }
            }

            return altitude;
        }

       


        public BoundingBox boundingBox;
        public Vector3[] corners;

        public Model model { get; set; }
        public Vector3 location { get { return Location; } set { this.Location = value;  } }
        private Vector3 Location;
        public Vector3 Rotation;

        public Vector3 scale { get; set; }

        public Texture2D[] texture;

        public Matrix projectionMatrix { get; set; }


        public bool IsTransparant { get; set; }
        public bool CanPlace { get; set; }

        public bool IsGhost { get; set; }
        


        public bool ShowBoundingBox = true;



        // // prelighting
        public class MeshTag
        {
            public Vector3 Color;
            public Texture2D Texture;
            public float SpecularPower;
            public Effect CachedEffect = null;

            public MeshTag(Vector3 Color, Texture2D Texture, float SpecularPower)
            {
                this.Color = Color;
                this.Texture = Texture;
                this.SpecularPower = SpecularPower;
            }
        }
    }
}
