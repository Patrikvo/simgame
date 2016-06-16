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


namespace Simgame2
{
    public class Camera : Simulation.Events.EventReceiver
    {

        public float DrawDistance
        {
            get { return _DrawDistance; }
            set 
            { 
                _DrawDistance = value;
                UpdateProjectionMatrix();
            }
        }

        

        public Camera(float AspectRatio)
        {
            this.AspectRatio = AspectRatio;
            _DrawDistance = 1200.0f;
            this.viewMatrix = Matrix.CreateLookAt(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -1));     /// here
            UpdateProjectionMatrix();
            MoveCamera(new Vector3(1600, 25, 500));
            this.cameraHeight = CameraHeightOffset;

        }

        public bool OnEvent(Simulation.Event ReceivedEvent)
        {
            return false;

        }


        private void UpdateProjectionMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.AspectRatio, 1.0f, DrawDistance);
            BigProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.AspectRatio, 1.0f, DrawDistance*10);
        }


        public BoundingFrustum GetFrustrum()
        {
            return new BoundingFrustum(this.viewMatrix * this.projectionMatrix);
        }
        

        public void MoveCamera(Vector3 newLocation)
        {
            this.cameraPosition = newLocation;
        }

        public Vector3 GetCameraPostion() { return this.cameraPosition; }

        public void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            MoveCamera(GetCameraPostion() + moveSpeed * rotatedVector);

            float camX = GetCameraPostion().X;
            float camZ = GetCameraPostion().Z;

            if (camX < 0) { camX = 0; }
            //if (camX > (worldMap.getMapWidth())) { camX = (worldMap.getMapWidth()); }
            if (camX > (LODMap.getMapWidth())) { camX = (LODMap.getMapWidth()); }
            
/*
            if (camZ > 0) { camZ = 0; }
            //if (camZ < -((worldMap.getMapHeight()) - 1)) { camZ = -((worldMap.getMapHeight()) - 1); }
            if (camZ < -((LODMap.getMapHeight()) - 1)) { camZ = -((LODMap.getMapHeight()) - 1); }
            */

            if (camZ < 0) { camZ = 0; }
            if (camZ > ((LODMap.getMapHeight()) - 1)) { camZ = ((LODMap.getMapHeight()) - 1); }

            MoveCamera(new Vector3(camX, (float)cameraHeight, camZ));
            UpdateViewMatrix();
        }

        public Vector3 GetPointBehindCamera(float distance)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(new Vector3(0, 0, 1), cameraRotation);
            return (GetCameraPostion() + distance * rotatedVector);
        }



        public void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(this.updownRot) * Matrix.CreateRotationY(this.leftrightRot);
            //Matrix cameraRotation = Matrix.CreateRotationX(-MathHelper.Pi/6) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);    // here
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = this.GetCameraPostion() + cameraRotatedTarget;
            LookAt = cameraRotatedTarget; // cameraFinalTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            this.viewMatrix = Matrix.CreateLookAt(this.GetCameraPostion(), cameraFinalTarget, cameraRotatedUpVector);



            
            Vector3 rotatedVector = Vector3.Transform(new Vector3(0,-1,0), cameraRotation);
        //    this.viewMatrixBackShifted = Matrix.CreateLookAt(this.GetCameraPostion() + 100 * rotatedVector, cameraFinalTarget, cameraRotatedUpVector);
            this.viewMatrixBackShifted = Matrix.CreateLookAt(this.GetPointBehindCamera(100), cameraFinalTarget, cameraRotatedUpVector);

            Vector3 reflCameraPosition = this.GetCameraPostion();
            //flCameraPosition.Y = -this.GetCameraPostion().Y + (WorldMap.waterHeight * 2);
            reflCameraPosition.Y = -this.GetCameraPostion().Y + (LODTerrain.LODTerrain.waterHeight * 2);
            Vector3 reflTargetPos = cameraFinalTarget;
            //reflTargetPos.Y = -cameraFinalTarget.Y + (WorldMap.waterHeight * 2);
            reflTargetPos.Y = -cameraFinalTarget.Y + (LODTerrain.LODTerrain.waterHeight * 2);


            Vector3 forwardVector = reflTargetPos - reflCameraPosition;
            sideVector = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 reflectionCamUp = Vector3.Cross(sideVector, forwardVector);
            //worldMap.GetRenderer().reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, reflectionCamUp);
            LODMap.GetRenderer().reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, reflectionCamUp);

            

        }


        public void ForceViewMatrix(Vector3 position, Vector3 target, Vector3 upVector)
        {
            this.viewMatrix = Matrix.CreateLookAt(position, target, upVector);
        }




        public void AdjustCameraAltitude(GameTime gameTime)
        {
            float intendedCameraHeight = 0;

            if (DoFixedAltitude)
            {
                intendedCameraHeight = 75;
            }
            else
            {
                // keeps camera at a set height above the terrain.
                //double intendedCameraHeight = (worldMap.getCellHeightFromWorldCoor(this.GetCameraPostion().X, -this.GetCameraPostion().Z)) + Camera.CameraHeightOffset;
                intendedCameraHeight = (LODMap.getCellHeightFromWorldCoor(this.GetCameraPostion().X, this.GetCameraPostion().Z)) + Camera.CameraHeightOffset;

            }
            

            

            
            // int intendedCameraHeight = worldMap.getAltitude(cameraPosition.X, cameraPosition.Z) + CameraHeightOffset;

            //if (intendedCameraHeight < (WorldMap.waterHeight + CameraHeightOffset))
            if (intendedCameraHeight < (LODTerrain.LODTerrain.waterHeight + CameraHeightOffset))
            {
             //   intendedCameraHeight = WorldMap.waterHeight + CameraHeightOffset;
                intendedCameraHeight = LODTerrain.LODTerrain.waterHeight + CameraHeightOffset;
            }


            if (this.cameraHeight < intendedCameraHeight)
            {
                this.cameraHeight += (float)(riseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                if (this.cameraHeight > intendedCameraHeight)
                {
                    this.cameraHeight = intendedCameraHeight;
                }
            }
            else if (this.cameraHeight > intendedCameraHeight)
            {
                this.cameraHeight -= (float)(dropSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                if (this.cameraHeight < intendedCameraHeight)
                {
                    this.cameraHeight = intendedCameraHeight;
                }
            }
        }


        public Ray UnProjectScreenPoint(float mouseX, float mouseY, Viewport vp) 
        {

            Vector3 nearSource = new Vector3(mouseX, mouseY, 0f);
            Vector3 farSource = new Vector3(mouseX, mouseY, 1f);

            Vector3 nearPoint = vp.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = vp.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
            /*
            //  mouse location in 3D landscape
            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.
            
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(mouseX + bias.X, mouseY + bias.Y, 0), this.projectionMatrix, this.viewMatrix, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(mouseX + bias.X, mouseY + bias.Y, 1), this.projectionMatrix, this.viewMatrix, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            // pos1 mouse cursor location at near clip plane
            // pos2 mouse cursor location at far clip plane
            // dir direction of line from pos1 to pos 2 
            
            //  If the mouse ray is aimed parallel with the world plane, then don't 
            //  intersect, because that would divide by zero.
            if (dir.Y != 0)
            {
                Vector3 x = pos1 - dir * (pos1.Y / dir.Y);

                markerLocation = x;

            }

            return markerLocation;*/
        }


        public Vector3 UnProjectScreenPointLoc(float mouseX, float mouseY, Viewport vp)
        {
               Vector3 markerLocation = Vector3.Zero;

               Vector2 bias = new Vector2(0, 0);

           
            //  mouse location in 3D landscape
            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.
            
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(mouseX + bias.X, mouseY + bias.Y, 0), this.projectionMatrix, this.viewMatrix, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(mouseX + bias.X, mouseY + bias.Y, 1), this.projectionMatrix, this.viewMatrix, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            // pos1 mouse cursor location at near clip plane
            // pos2 mouse cursor location at far clip plane
            // dir direction of line from pos1 to pos 2 
            
            //  If the mouse ray is aimed parallel with the world plane, then don't 
            //  intersect, because that would divide by zero.
            if (dir.Y != 0)
            {
                Vector3 x = pos1 - dir * (pos1.Y / dir.Y);

                markerLocation = x;

            }

            return markerLocation;
        }


        public void Store(GameSession.GameStorage writer)
        {
            writer.Write(leftrightRot);
            writer.Write(updownRot);

            writer.Write(cameraPosition);

            writer.Write(cameraHeight);

            writer.Write(LookAt);

            writer.Write(sideVector);

            writer.Write(_DrawDistance);
            writer.Write(AspectRatio);

            writer.Write(viewMatrix);

            writer.Write(viewMatrixBackShifted);

            writer.Write(projectionMatrix);

            writer.Write(BigProjectionMatrix);
        }

        public void Restore(GameSession.GameStorage reader)
        {


            leftrightRot = reader.ReadSingle();
            updownRot = reader.ReadSingle();

            cameraPosition = reader.ReadVector3(); 

            cameraHeight = reader.ReadSingle();

            LookAt = reader.ReadVector3();

            sideVector = reader.ReadVector3();

            _DrawDistance = reader.ReadSingle();
            AspectRatio = reader.ReadSingle();

            viewMatrix = reader.ReadMatrix();

            viewMatrixBackShifted = reader.ReadMatrix();

            projectionMatrix = reader.ReadMatrix();

            BigProjectionMatrix = reader.ReadMatrix();
        }



        public Matrix viewMatrix { get; set; }
        public Matrix viewMatrixBackShifted{ get; set; }

        private Vector3 cameraPosition;
        public float leftrightRot = 4 * MathHelper.Pi;
        public float updownRot = -MathHelper.Pi / 10.0f;


        public float cameraHeight { get; set; }


        public const int CameraHeightOffset = 25;


        public Matrix projectionMatrix;
        public Matrix BigProjectionMatrix;


        private float _DrawDistance;
        public float AspectRatio;


        public Vector3 LookAt;
        public Vector3 sideVector;




        
        public const float moveSpeed = 250.0f; // 80.0f;
        public const float riseSpeed = 160.0f;
        public const float dropSpeed = 30.0f;
        public const float rotationSpeed = 0.3f;

        public LODTerrain.LODTerrain LODMap { get; set; }

        public bool DoFixedAltitude;
        public int FixedAltitude = 75;



    }
}
