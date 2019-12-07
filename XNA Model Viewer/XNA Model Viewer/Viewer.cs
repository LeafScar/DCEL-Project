#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DCEL;
#endregion

namespace Primitives3D
{
    /// <summary>
    /// This sample shows how to draw 3D geometric primitives
    /// such as cubes, spheres, and cylinders.
    /// </summary>
    public class Viewer : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;
        MouseState currentMouseState;
        MouseState lastMouseState;

        // Store a list of primitive models, plus which one is currently selected.
        List<GeometricPrimitive> primitives = new List<GeometricPrimitive>();

        int currentPrimitiveIndex = 0;

        // store a wireframe rasterize state
        RasterizerState wireFrameState;

        // Store a list of tint colors, plus which one is currently selected.
        List<Color> colors = new List<Color>
        {
            Color.Red,
            Color.Blue,
            Color.White,
            Color.Green,
            Color.DarkOrange,
        };

        int currentColorIndex = 0;

        // Are we rendering in wireframe mode?
        bool isWireframe;


        #endregion

        #region Initialization


        public Viewer()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //spriteFont = Content.Load<SpriteFont>("hudfont");

            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/apple.off"), 0.005f));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/cow.off"), 1));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/cube.off"), 0.5f));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/octtorus.off"), 0.33f));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/epcot.off"), 0.75f));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/homer.off"), 2));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/mushroom.off"), 1));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/seashell.off"), 2));
            primitives.Add(new DCELPrimitive(GraphicsDevice, new DCELMesh(@"shapes/space_station.off"), 0.15f));

            wireFrameState = new RasterizerState()
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None,
            };

        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (isWireframe)
            {
                GraphicsDevice.RasterizerState = wireFrameState;
            }
            else
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            }

            // Create camera matrices, making the object spin.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            Vector3 cameraPosition = new Vector3(0, 0, 2.5f);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            Matrix world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 50);

            // Draw the current primitive.
            GeometricPrimitive currentPrimitive = primitives[currentPrimitiveIndex];
            Color color = colors[currentColorIndex];

            currentPrimitive.Draw(world, view, projection, color);

            // Reset the fill mode renderstate.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw overlay text.
            string text = "A or tap top of screen = Change primitive\n" +
                          "B or tap bottom left of screen = Change color\n" +
                          "Y or tap bottom right of screen = Toggle wireframe";

            //spriteBatch.Begin();
            //spriteBatch.DrawString(spriteFont, text, new Vector2(30, 30), Color.White);
            //spriteBatch.DrawString(null, text, new Vector2(30, 30), Color.White);
            //spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting or changing settings.
        /// </summary>
        void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastMouseState = currentMouseState;

#if WINDOWS_PHONE
            currentKeyboardState = new KeyboardState();
#else
            currentKeyboardState = Keyboard.GetState();
#endif
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

            // Check for exit.
            if (IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }

            // Change primitive?
            Viewport viewport = GraphicsDevice.Viewport;
            int halfWidth = viewport.Width / 2;
            int halfHeight = viewport.Height / 2;
            Rectangle topOfScreen = new Rectangle(0, 0, viewport.Width, halfHeight);
            if (IsPressed(Keys.A, Buttons.A) || LeftMouseIsPressed(topOfScreen))
            {
                currentPrimitiveIndex = (currentPrimitiveIndex + 1) % primitives.Count;
            }


            // Change color?
            Rectangle botLeftOfScreen = new Rectangle(0, halfHeight, halfWidth, halfHeight);
            if (IsPressed(Keys.B, Buttons.B) || LeftMouseIsPressed(botLeftOfScreen))
            {
                currentColorIndex = (currentColorIndex + 1) % colors.Count;
            }


            // Toggle wireframe?
            Rectangle botRightOfScreen = new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight);
            if (IsPressed(Keys.Y, Buttons.Y) || LeftMouseIsPressed(botRightOfScreen))
            {
                isWireframe = !isWireframe;
            }
        }


        /// <summary>
        /// Checks whether the specified key or button has been pressed.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                    lastKeyboardState.IsKeyUp(key)) ||
                   (currentGamePadState.IsButtonDown(button) &&
                    lastGamePadState.IsButtonUp(button));
        }

        bool LeftMouseIsPressed(Rectangle rect)
        {
            return (currentMouseState.LeftButton == ButtonState.Pressed &&
                    lastMouseState.LeftButton != ButtonState.Pressed &&
                    rect.Contains(currentMouseState.X, currentMouseState.Y));
        }

        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (Viewer game = new Viewer())
            {
                game.Run();
            }
        }
    }

    #endregion
}