using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Simgame2.Tools
{
    public partial class FormDebug : Form
    {
        protected GameSession.GameSession RunningGameSession;

        public FormDebug(GameSession.GameSession RunningGameSession)
        {
            
            this.RunningGameSession = RunningGameSession;

            InitializeComponent();




            this.trackBarYaw.Value = (int)(this.RunningGameSession.LODMap.GetRenderer().SunLight.Yaw / (2 * Math.PI) * 360);
            this.trackBarPitch.Value = (int)(this.RunningGameSession.LODMap.GetRenderer().SunLight.Pitch / (2 * Math.PI) * 360);
            this.trackBarRoll.Value = (int)(this.RunningGameSession.LODMap.GetRenderer().SunLight.Roll / (2 * Math.PI) * 360);

            this.labelLightvalues.Text = "yaw: " + this.RunningGameSession.LODMap.GetRenderer().SunLight.Yaw.ToString("0.00") + " / pitch: " +
                this.RunningGameSession.LODMap.GetRenderer().SunLight.Pitch.ToString("0.00") + " / roll: " + this.RunningGameSession.LODMap.GetRenderer().SunLight.Roll.ToString("0.00");


            this.textBoxShadowPosX.Text = this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightPosition.X.ToString();
            this.textBoxShadowPosY.Text = this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightPosition.Y.ToString();
            this.textBoxShadowPosZ.Text = this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightPosition.Z.ToString();

            this.textBoxlShadowTargetX.Text = this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightTarget.X.ToString();
            this.textBoxlShadowTargetY.Text = this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightTarget.Y.ToString();
            this.textBoxlShadowTargetZ.Text = this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightTarget.Z.ToString();

            this.textBoxShadowFarPlane.Text = this.RunningGameSession.LODMap.GetRenderer().shadowFarPlane.ToString();

            this.textBoxShadowBias.Text = (1.0f / this.RunningGameSession.LODMap.GetRenderer().ShadowBias).ToString();

            this.textBoxNormalBias.Text = this.RunningGameSession.LODMap.GetRenderer().NormalBias.ToString();





        



        }






        


        public void writeline(string line)
        {
            textBox.Text += line + Environment.NewLine;
            
        }

        public void write(string line)
        {
            textBox.Text += line;
        }

        public void clear()
        {
            textBox.Text = String.Empty;
        }

        private void trackBarMainLight_Scroll(object sender, EventArgs e)
        {
            double yaw = ((float)trackBarYaw.Value) / 360 * 2 * Math.PI;
            double pitch = ((float)trackBarPitch.Value) / 360 * 2 * Math.PI;
            double roll = ((float)trackBarRoll.Value) / 360 * 2 * Math.PI;

            this.labelLightvalues.Text = "yaw: " + yaw.ToString("0.00") + " / pitch: " + pitch.ToString("0.00") + " / roll: " + roll.ToString("0.00");

            this.RunningGameSession.LODMap.GetRenderer().SunLight.SetLightDirection((float)yaw, (float)pitch, (float)roll);
            //this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightPosition
        }

       

        private void textBoxShadowPos_Leave(object sender, EventArgs e)
        {
            float x, y, z;
            bool allOk = true;
            if (!ParseFloatField(this.textBoxShadowPosX.Text, this.labelShadowPosX, out x))
            {
                allOk = false;
            }

            if (!ParseFloatField(this.textBoxShadowPosY.Text, this.labelShadowPosY, out y))
            {
                allOk = false;
            }

            if (!ParseFloatField(this.textBoxShadowPosZ.Text, this.labelShadowPosZ, out z))
            {
                allOk = false;
            }


            if (allOk) 
            {
                 Microsoft.Xna.Framework.Vector3 shadowPos = new Microsoft.Xna.Framework.Vector3(x, y, z);
                 this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightPosition = shadowPos;
            }

        }

        private bool ParseFloatField(string text, Label errorIndicator, out float result)
        {
            if (float.TryParse(text, out result))
            {
                errorIndicator.ForeColor = Color.Black;
                return true; 
            }

            errorIndicator.ForeColor = Color.Red;
            return false;
        }

        private bool ParseIntField(string text, Label errorIndicator, out int result)
        {
            if (int.TryParse(text, out result))
            {
                errorIndicator.ForeColor = Color.Black;
                return true;
            }

            errorIndicator.ForeColor = Color.Red;
            return false;
        }

        private void textBoxlShadowTarget_Leave(object sender, EventArgs e)
        {
            //lShadowTargetX
            float x, y, z;
            bool allOk = true;
            if (!ParseFloatField(this.textBoxlShadowTargetX.Text, this.labelShadowTargetX, out x))
            {
                allOk = false;
            }

            if (!ParseFloatField(this.textBoxlShadowTargetY.Text, this.labellShadowTargetY, out y))
            {
                allOk = false;
            }

            if (!ParseFloatField(this.textBoxlShadowTargetZ.Text, this.labellShadowTargetZ, out z))
            {
                allOk = false;
            }


            if (allOk)
            {
                Microsoft.Xna.Framework.Vector3 shadowPos = new Microsoft.Xna.Framework.Vector3(x, y, z);
                this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightTarget = shadowPos;
            }

        }

        private void textBoxShadowFarPlane_Leave(object sender, EventArgs e)
        {
            // ShadowFarPlane
            int farPlane;
            if (ParseIntField(this.textBoxShadowFarPlane.Text, this.labelShadowFarPlane, out farPlane))
            {
                this.RunningGameSession.LODMap.GetRenderer().shadowFarPlane = farPlane;
            }

        }

        private void textBoxShadowBias_Leave(object sender, EventArgs e)
        {
            int bias;
            if (ParseIntField(this.textBoxShadowBias.Text, this.labelShadowBias, out bias))
            {
                this.RunningGameSession.LODMap.GetRenderer().ShadowBias = 1.0f / bias;
            }
        }

        private void textBoxNormalBias_Leave(object sender, EventArgs e)
        {
            float bias;
            if (ParseFloatField(this.textBoxNormalBias.Text, this.labelNormalBias, out bias))
            {
                this.RunningGameSession.LODMap.GetRenderer().NormalBias = bias;
            }
        }

      




        // NormalBias

    }
}
