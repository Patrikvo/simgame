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
    public class GUI
    {
        // TODO allow mouse button clicking a button actually do something (now it is just a visible action)

        private Game1 game;

        public GUI(Game1 game)
        {
            this.buttonWidth = 100;
            this.buttonHeight = 100;


            this.upper = game.GraphicsDevice.Viewport.Height - this.buttonHeight;
            this.left = 0;

            this.game = game;
        }

        public void Initialize()
        {
        }

        public void Update(int mouseX, int mouseY, bool leftMouseButtonPressed)
        {
            for (int i = 0; i < this.buttons.Length; i++)
            {
                buttons[i].Update(mouseX, mouseY, leftMouseButtonPressed);
            }
        }


        public void Draw(GameTime gameTime, GraphicsDevice device)
        {
            SpriteBatch batch = new SpriteBatch(device);
            batch.Begin();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Draw(gameTime, batch);
                
            }
            batch.End();
        }

        public void doNothing()
        {
        }

        public void PlaceBuilding()
        {
            this.game.ChangeGameState(Game1.GameInputState.PLACE_BUILDING);
        }

        public void AddButton(Texture2D imageReleased, Texture2D imagePressed)
        {
            if (buttons == null)
            {
                buttons = new Button[1];
                buttons[0] = new Button(imageReleased, imagePressed, this.upper, this.left, this.buttonWidth, this.buttonHeight);
                buttons[0].MouseClick += PlaceBuilding;
            }
            else
            {
                // quick and dirty exanding array (inefficient, but only used once)
                Button[] newButtons = new Button[buttons.Length + 1];

                for (int i = 0; i < buttons.Length; i++)
                {
                    newButtons[i] = buttons[i];
                }
                newButtons[buttons.Length] = new Button(imageReleased, imagePressed, this.upper, buttons.Length * this.buttonWidth, this.buttonWidth, this.buttonHeight);
                newButtons[buttons.Length].MouseClick += doNothing;
                buttons = newButtons;
            }


        }


        private Button[] buttons = null;
        private int upper;
        private int left;
        private int buttonWidth;
        private int buttonHeight;



        private class Button
        {
            public bool Pressed = false;
            public bool MouseOver = false;
            public int Upper;
            public int Left;
            public int Width;
            public int Height;
            public Texture2D Image_released;
            public Texture2D Image_pressed;

            

            public Button(Texture2D imgReleased, Texture2D imgPressed, int upper, int left, int width, int height)
            {
                this.Image_released = imgReleased;
                this.Image_pressed = imgPressed;
                this.Upper = upper;
                this.Left = left;
                this.Width = width;
                this.Height = height;
            }

            

            public void Update(int mouseX, int mouseY, bool LeftButtonDown)
            {
                // is mouse cursor over this image?
                this.MouseOver = false;
                
                if (mouseX > this.Left && mouseX < (this.Left + this.Width) && mouseY > this.Upper && mouseY < (this.Upper + this.Height))
                {
                    this.MouseOver = true;

                    if (LeftButtonDown == true)
                    {
                        this.Pressed = true;
                    }
                    else
                    {
                        if (this.Pressed == true)
                        {
                            MouseClick();
                            this.Pressed = false;
                        }
                    }
                }
            }

            public void Draw(GameTime gameTime, SpriteBatch batch)
            {
                Vector2 pos = new Vector2(this.Left, this.Upper);
                Color color;
                if (this.MouseOver) 
                {
                    color = Color.Orange;
                }
                else
                {
                    color = Color.White;
                }
                if (this.Pressed)
                {
                    batch.Draw(Image_pressed, pos, color);
                }
                else
                {
                    batch.Draw(Image_released, pos, color);
                }
            }

            public Click MouseClick;

            public delegate void Click();

        }

    }
}
