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
    public class GUI : Simulation.Events.EventReceiver
    {
        const string ARROWL = "GUI\\Button_arrowL";
        const string ARROWL_P = "GUI\\Button_arrowL_P";
        const string SOLAR = "GUI\\Button_Solar";
        const string SOLOR_P = "GUI\\Button_Solar_P";
        const string MELTER = "GUI\\Button_melter";
        const string MELLTER_P = "GUI\\Button_melter_P";
        const string MINE = "GUI\\Button_mine";
        const string MINE_P = "GUI\\Button_mine_P";
        const string ARROWR = "GUI\\Button_arrowR";
        const string ARROWR_P = "GUI\\Button_arrowR_P";

        
        // TODO
        // display resource list from RunningGameSession.AvailableResources


        List<Texture2D> images;
        int resourceWindowWidth = 400;
        int resourceWindowHeight = 800;
        int resourceWindowX = 0;
        int resourceWindowY = 100;


        Texture2D dummyTexture;
        Rectangle dummyRectangle;

        protected GameSession.GameSession RunningGameSession;

        public GUI(GameSession.GameSession RunningGameSession)
        {
            this.buttonWidth = 100;
            this.buttonHeight = 100;

            this.RunningGameSession = RunningGameSession;

            this.upper = this.RunningGameSession.device.Viewport.Height - this.buttonHeight;
            this.left = 0;

            ShowResourceWindow = false;

            images = new List<Texture2D>();

            dummyTexture = new Texture2D(this.RunningGameSession.device, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
            dummyRectangle = new Rectangle(resourceWindowX, resourceWindowY, resourceWindowWidth, resourceWindowHeight);

        }

        public bool OnEvent(Simulation.Event ReceivedEvent)
        {
            return false;
        }


        public void Initialize()
        {
        }

        public bool ShowResourceWindow { get; set; }
        public SpriteFont font { get { return this.RunningGameSession.font; } }

        public void PreloadImages(ContentManager Content)
        {
            this.images.Add(Content.Load<Texture2D>(ARROWL));
            this.images.Add(Content.Load<Texture2D>(ARROWL_P));
            this.images.Add(Content.Load<Texture2D>(SOLAR));
            this.images.Add(Content.Load<Texture2D>(SOLOR_P));
            this.images.Add(Content.Load<Texture2D>(MELTER));
            this.images.Add(Content.Load<Texture2D>(MELLTER_P));
            this.images.Add(Content.Load<Texture2D>(MINE));
            this.images.Add(Content.Load<Texture2D>(MINE_P));
            this.images.Add(Content.Load<Texture2D>(ARROWR));
            this.images.Add(Content.Load<Texture2D>(ARROWR_P));
        }

        public void ConstructButtons()
        {
            AddButton(0, 1, PlaceBuildingFoundation);  // Arrow L
            AddButton(2, 3, PlaceBuildingSolar);  // SOLAR
            AddButton(4, 5, PlaceBuildingMelter); // MELTER
            AddButton(6, 7, PlaceBuildingMine); // MINE
            AddButton(8, 9, PlaceBuildingWind); // ARROW R
        }



        public void Update(int mouseX, int mouseY, bool leftMouseButtonPressed, bool hide)
        {
            if (!hide)
            {
                for (int i = 0; i < this.buttons.Length; i++)
                {
                    buttons[i].Update(mouseX, mouseY, leftMouseButtonPressed);
                }
                
            }
        }


        public void Draw(GameTime gameTime, GraphicsDevice device)
        {
            SpriteBatch batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
           // batch.Begin();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Draw(gameTime, batch);
                
            }
            if (ShowResourceWindow)
            {
                DrawResourceWindow(gameTime, batch);
            }


            batch.End();
        }


        public void DrawResourceWindow(GameTime gameTime, SpriteBatch batch)
        {
            string resources = RunningGameSession.AvailableResources.GetResourceString();
            Vector2 size = this.font.MeasureString(resources);
            this.dummyRectangle = new Rectangle(resourceWindowX, resourceWindowY, (int)size.X+40, (int)size.Y+40);
            batch.Draw(dummyTexture, dummyRectangle, Color.White);

            batch.DrawString(this.font, resources, new Vector2(resourceWindowX + 20, resourceWindowY + 20), Color.Red);

        }



 


        public void doNothing()
        {
        }

        public void PlaceBuildingSolar() 
        {
            this.RunningGameSession.PlaceBuildingState.LastSelectedEntityType = Entity.EntityTypes.SOLAR;
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
        }

        public void PlaceBuildingMelter()
        {
            this.RunningGameSession.PlaceBuildingState.LastSelectedEntityType = Entity.EntityTypes.MELTER;
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
        }

        public void PlaceBuildingMine()
        {
            this.RunningGameSession.PlaceBuildingState.LastSelectedEntityType = Entity.EntityTypes.BASIC_MINE;
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
        }

        public void PlaceBuildingWind()
        {
            this.RunningGameSession.PlaceBuildingState.LastSelectedEntityType = Entity.EntityTypes.WIND_TOWER;
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
        }

        public void PlaceBuildingFoundation()
        {
            this.RunningGameSession.PlaceBuildingState.LastSelectedEntityType = Entity.EntityTypes.FOUNDATION;
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
        }

        public void PlaceBuilding()
        {
            this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
        }

        public void AddButton(int imageReleaseID, int imagePressedID, Click clickDelegate)
        {
            this.AddButton(images[imageReleaseID], images[imagePressedID], clickDelegate);
        }

        public void AddButton(Texture2D imageReleased, Texture2D imagePressed, Click clickDelegate)
        {
            if (buttons == null)
            {
                buttons = new Button[1];
                buttons[0] = new Button(imageReleased, imagePressed, this.upper, this.left, this.buttonWidth, this.buttonHeight);
                //buttons[0].MouseClick += PlaceBuilding;
                buttons[0].MouseClick += clickDelegate;
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

                //newButtons[buttons.Length].MouseClick += doNothing;
                newButtons[buttons.Length].MouseClick += clickDelegate;

                buttons = newButtons;
            }


        }


        private Button[] buttons = null;
        private int upper;
        private int left;
        private int buttonWidth;
        private int buttonHeight;


        public delegate void Click();

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

            

        }


        
    }
}
