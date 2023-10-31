using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Aristurtle.MonoGame.Save;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PngExample;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _aristurlte;
    private float _rotation = 0.0f;
    private Vector2 _centerOfScreen;
    private Vector2 _centerOfTexture;
    private KeyboardState _prevKey;
    private KeyboardState _curKey;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        _centerOfScreen = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight) * 0.5f;
        _centerOfTexture = new Vector2(_aristurlte.Width, _aristurlte.Height) * 0.5f;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _aristurlte = Content.Load<Texture2D>("aristurtle");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _prevKey = _curKey;
        _curKey = Keyboard.GetState();

        //  Press enter to save
        if(_curKey.IsKeyDown(Keys.Enter) && _prevKey.IsKeyUp(Keys.Enter))
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Game.Save.png");
            SaveModel model = new SaveModel()
            {
                Rotation = _rotation
            };
            string json = JsonSerializer.Serialize<SaveModel>(model);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            Color[] pixels = new Color[_graphics.PreferredBackBufferWidth * _graphics.PreferredBackBufferHeight];
            GraphicsDevice.GetBackBufferData<Color>(pixels);

            SaveFileWriter.ToPng(path, data, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, pixels);
        }
        //  Press Space to load
        else if(_curKey.IsKeyDown(Keys.Space) && _prevKey.IsKeyUp(Keys.Space))
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Game.Save.png");
            byte[] data = SaveFileReader.FromPng(path);
            string json = System.Text.Encoding.UTF8.GetString(data);
            SaveModel model = JsonSerializer.Deserialize<SaveModel>(json);
            _rotation = model.Rotation;
        }


        _rotation += 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_aristurlte, _centerOfScreen, null, Color.White, _rotation, _centerOfTexture, 1.0f, SpriteEffects.None, 0.0f);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
