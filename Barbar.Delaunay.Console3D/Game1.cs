using Barbar.Delaunay.Drawing;
using Barbar.Delaunay.Examples;
using Barbar.Delaunay.Voronoi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Linq;

namespace Barbar.Delaunay.Console3D
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Effect basicEffect;
        VertexBuffer terrain;
        float scale = 1.0f;
        int meshTriangles = 0;

        Matrix world
        {
            get
            {
                return Matrix.CreateScale(scale) * Matrix.CreateTranslation(FocusPoint);
            }
        }

        Vector3 FocusPoint = new Vector3(-0.5f, 0.0f, -0.5f);


        Matrix view
        {
            get
            {
                float d = (float)Math.Sqrt(1f / 3f);
                return Matrix.CreateLookAt(new Vector3(d, d, d), Vector3.Zero, Vector3.Up);
            }
        }

        //Matrix view = Matrix.CreateLookAt(new Vector3(500, 500, 50), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //basicEffect = Content.Load<Effect>("specular");
            basicEffect = Content.Load<Effect>("diffuse");

            var graph = SampleGenerator.CreateVoronoiGraph(1000, 30000, 2, 23);
            var vertices = graph.Paint3D(new VertexPositionColorNormalFactory());

            var normal = new Vector3(0, -1, 0);

            meshTriangles = vertices.Count / 3;

            vertices.Add(new VertexPositionColorNormal(Vector3.Zero, Color.Red, normal));
            vertices.Add(new VertexPositionColorNormal(new Vector3(1.15f, 0, 0), Color.Red, normal));
            vertices.Add(new VertexPositionColorNormal(Vector3.Zero, Color.Green, normal));
            vertices.Add(new VertexPositionColorNormal(new Vector3(0, 1.15f, 0), Color.Green, normal));
            vertices.Add(new VertexPositionColorNormal(Vector3.Zero, Color.Blue, normal));
            vertices.Add(new VertexPositionColorNormal(new Vector3(0, 0, 1.15f), Color.Blue, normal));

            var rawVertices = vertices.ToArray();
            terrain = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorNormal), rawVertices.Length, BufferUsage.WriteOnly);
            terrain.SetData(rawVertices);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delta = 0.001f;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                FocusPoint += new Vector3(0, 0, delta);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                FocusPoint += new Vector3(0, 0, -delta);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                FocusPoint += new Vector3(delta, 0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                FocusPoint += new Vector3(-delta, 0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                scale += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                scale -= 0.1f;
            }


            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //basicEffect.EnableDefaultLighting();
            //basicEffect.PreferPerPixelLighting = true;


            //basicEffect.CurrentTechnique = basicEffect.Techniques["Simplest"];
            basicEffect.Parameters["View"].SetValue(view);
            basicEffect.Parameters["Projection"].SetValue(projection);
            basicEffect.Parameters["World"].SetValue(world);
            basicEffect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            //basicEffect.Parameters["xViewProjection"].SetValue(view * projection);

            /*
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;*/
            //basicEffect.EnableDefaultLighting();

            GraphicsDevice.SetVertexBuffer(terrain);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;


            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, meshTriangles);
                //GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, meshTriangles * 3, meshTriangles * 3);
                GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, terrain.VertexCount - 6, 3);
            }


            base.Draw(gameTime);
        }
    }
}
