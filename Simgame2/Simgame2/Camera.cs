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
    public class Camera
    {
        public Camera(float AspectRatio)
        {
            this.viewMatrix = Matrix.CreateLookAt(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -1));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, AspectRatio, 1.0f, 300.0f);
            MoveCamera(new Vector3(0, 25, 0));
            this.cameraHeight = CameraHeightOffset;

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
            if (camX > (worldMap.getMapWidth())) { camX = (worldMap.getMapWidth()); }

            if (camZ > 0) { camZ = 0; }
            if (camZ < -((worldMap.getMapHeight()) - 1)) { camZ = -((worldMap.getMapHeight()) - 1); }

            MoveCamera(new Vector3(camX, (float)cameraHeight, camZ));
            UpdateViewMatrix();
        }


        public void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(this.updownRot) * Matrix.CreateRotationY(this.leftrightRot);
            //Matrix cameraRotation = Matrix.CreateRotationX(-MathHelper.Pi/6) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = this.GetCameraPostion() + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            this.viewMatrix = Matrix.CreateLookAt(this.GetCameraPostion(), cameraFinalTarget, cameraRotatedUpVector);


            Vector3 reflCameraPosition = this.GetCameraPostion();
            reflCameraPosition.Y = -this.GetCameraPostion().Y + (WorldMap.waterHeight * 2);
            Vector3 reflTargetPos = cameraFinalTarget;
            reflTargetPos.Y = -cameraFinalTarget.Y + (WorldMap.waterHeight * 2);


            Vector3 forwardVector = reflTargetPos - reflCameraPosition;
            Vector3 sideVector = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 reflectionCamUp = Vector3.Cross(sideVector, forwardVector);
            worldMap.reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, reflectionCamUp);




        }



        public void AdjustCameraAltitude(GameTime gameTime)
        {
            // keeps camera at a set height above the terrain.
            int intendedCameraHeight = (worldMap.getCellFromWorldCoor(this.GetCameraPostion().X, -this.GetCameraPostion().Z)) + Camera.CameraHeightOffset;
            // int intendedCameraHeight = worldMap.getAltitude(cameraPosition.X, cameraPosition.Z) + CameraHeightOffset;

            if (this.cameraHeight < intendedCameraHeight)
            {
                this.cameraHeight += riseSpeed * gameTime.ElapsedGameTime.TotalSeconds;
                if (this.cameraHeight > intendedCameraHeight)
                {
                    this.cameraHeight = intendedCameraHeight;
                }
            }
            else if (this.cameraHeight > intendedCameraHeight)
            {
                this.cameraHeight -= dropSpeed * gameTime.ElapsedGameTime.TotalSeconds;
                if (this.cameraHeight < intendedCameraHeight)
                {
                    this.cameraHeight = intendedCameraHeight;
                }
            }
        }


        public Vector3 UnProjectScreenPoint(float mouseX, float mouseY, Viewport vp) 
        {
            Vector3 markerLocation = Vector3.Zero; 

            //  mouse location in 3D landscape
            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.
            
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(mouseX, mouseY, 0), this.projectionMatrix, this.viewMatrix, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(mouseX, mouseY, 1), this.projectionMatrix, this.viewMatrix, Matrix.Identity);
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


        public Matrix viewMatrix { get; set; }


        private Vector3 cameraPosition;
        public float leftrightRot = 4 * MathHelper.Pi;
        public float updownRot = -MathHelper.Pi / 10.0f;


        public double cameraHeight { get; set; }


        public const int CameraHeightOffset = 25;



        
        public const float moveSpeed = 80.0f;
        public const float riseSpeed = 60.0f;
        public const float dropSpeed = 30.0f;
        public const float rotationSpeed = 0.3f;

        public WorldMap worldMap { get; set; }

        //private WorldMap worldMap;

        public Matrix projectionMatrix;

    }
}
