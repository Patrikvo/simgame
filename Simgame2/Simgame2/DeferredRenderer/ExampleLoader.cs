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
    class ExampleLoader:Game
    {

        //Graphics Device
GraphicsDeviceManager graphics;
//SpriteBatch
SpriteBatch spriteBatch;
//Sprite Font
SpriteFont spriteFont;
//Keyboard State Holders
KeyboardState currentK;
KeyboardState previousK;
//Mouse State Holder
MouseState currentM;
//Camera
Camera Camera;
//Light Manager
LightManager lightManager;
//Deffered Renderer
DeferredRenderer deferredRenderer;
//SSAO
SSAO ssao;
//Scene
RenderTarget2D Scene;
//List of Models
List<Model> models;
public ExampleLoader()
{
graphics = new GraphicsDeviceManager(this);
Content.RootDirectory = "Content";
graphics.PreferredBackBufferWidth = 800;
graphics.PreferredBackBufferHeight = 600;
}
protected override void Initialize()
{
//Initialize Camera
//Camera = new Camera(this, new Vector3(0, 0, 5), new Vector3(0, 0, 0), Vector3.Up);
base.Initialize();
}
protected override void LoadContent()
{
// Create a new SpriteBatch, which can be used to draw textures.
spriteBatch = new SpriteBatch(GraphicsDevice);
//Create spriteFont
spriteFont = Content.Load<SpriteFont>("DefaultFont");
//Create Deferred Renderer
deferredRenderer = new DeferredRenderer(GraphicsDevice, Content, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
//Create SSAO
ssao = new SSAO(GraphicsDevice, Content, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
//Create Scene Render Target
Scene = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
//Create Light Manager
lightManager = new LightManager(Content);
//Load SpotLight Cookies
Texture2D spotCookie = Content.Load<Texture2D>("SpotCookie");
Texture2D squareCookie = Content.Load<Texture2D>("SquareCookie");
//Add a Directional Light
lightManager.AddLight(new DirectionalLight(Vector3.Down, Color.White, 0.05f));
//Add a Spot Light
lightManager.AddLight(new SpotLight(GraphicsDevice, new Vector3(0, 15.0f, 0), new Vector3(0, -1, 0), Color.White.ToVector4(), 0.10f, true, 2048, spotCookie));
//Add a Point Light
lightManager.AddLight(new PointLight(GraphicsDevice, new Vector3(0, 5.0f, 0), 20.0f, Color.White.ToVector4(), 0.80f, true, 256));
//Initialize Model List
models = new List<Model>();
//Load Models
Model scene = Content.Load<Model>("Scene");
models.Add(scene);
}
protected override void UnloadContent()
{
}
protected override void Update(GameTime gameTime)
{
//Input Housekeeping
previousK = currentK;
currentK = Keyboard.GetState();
currentM = Mouse.GetState();
Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
//Exit check
if (currentK.IsKeyUp(Keys.Escape) && previousK.IsKeyDown(Keys.Escape)) this.Exit();
//Update Camera
//Camera.Update(this, currentK, previousK, currentM);
//Modify
ssao.Modify(currentK);
base.Update(gameTime);
}
protected override void Draw(GameTime gameTime)
{
//Clear
GraphicsDevice.Clear(Color.CornflowerBlue);
//Draw Shadow Maps
lightManager.DrawShadowMaps(GraphicsDevice, models);
//Draw with SSAO unless F1 is down
if (currentK.IsKeyDown(Keys.F1))
{
//Draw using Deferred Renderer straight to BackBuffer
deferredRenderer.Draw(GraphicsDevice, models, lightManager, Camera, null);
}
else
{
//Draw using Deferred Renderer
deferredRenderer.Draw(GraphicsDevice, models, lightManager, Camera, Scene);
//Draw non blurred ssao
ssao.Draw(GraphicsDevice, deferredRenderer.getGBuffer(), Scene, Camera, null);
}
//Debug Deferred Renderer
deferredRenderer.Debug(GraphicsDevice, spriteBatch);
//Debug SSAO
ssao.Debug(spriteBatch, spriteFont);
//Base Drawing
base.Draw(gameTime);
}

    }
}
