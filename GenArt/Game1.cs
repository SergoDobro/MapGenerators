using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace GenArt
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        const int sc = 5; 
        Random random = new Random();
        Point MapData = new Point((int)(50 * Math.Pow(2, sc)), (int)(50 * Math.Pow(2, sc)));
        Color[] colors;
        Texture2D _texture;
        Texture2D _texture2;
        double[,] heightmap;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        List<Agent> agents;
        Texture2D OnePixeltexture;
        protected override void Initialize()
        {
             
            //_texture = new Texture2D(GraphicsDevice, MapData.X, MapData.Y);
            _texture = Texture2D.FromFile(GraphicsDevice, "pic.png");
            MapData.X = _texture.Width;
            MapData.Y = _texture.Height;
            _texture2 = new Texture2D(GraphicsDevice, MapData.X, MapData.Y);

            PerlinNoise.PerlinNoise.SetTexture(null, _texture2, MapData.X, MapData.Y, 20, scale: 16 * (int)Math.Pow(2, sc)); 

            //PerlinNoise.PerlinNoise.ScaleTall_Texture(_texture, (a)=>(byte)(255*Math.Clamp(Math.Pow(0.3+a/255f,6),0,1)));
            PerlinNoise.PerlinNoise.ScaleTall_Texture(_texture, (a)=>(byte)(255*Math.Clamp(Math.Pow(0.1+a/255f,6),0,1)));

            //using (StreamWriter wr = new StreamWriter("pic.png"))
            //{
            //    _texture.SaveAsPng(wr.BaseStream, MapData.X, MapData.Y);
            //}

            _graphics.PreferredBackBufferHeight = 1080;// _texture.Height;
            _graphics.PreferredBackBufferWidth = 1920;//_texture.Width;
            RenderTarget = new RenderTarget2D(GraphicsDevice, MapData.X, MapData.Y);
            Color[] colorsss = new Color[MapData.X * MapData.Y];
            for (int i = 0; i < MapData.X * MapData.Y; i++)
            {
                colorsss[i] = Color.Transparent;
            }
            RenderTarget.SetData(colorsss);
            _graphics.ApplyChanges();



            heightmap = new double[_texture.Width, _texture.Height];
            colors = new Color[_texture.Width * _texture.Height];
            _texture.GetData<Color>(colors);



            Color[] colors2 = new Color[_texture.Width * _texture.Height];
            _texture2.GetData<Color>(colors2);

            for (int i = 0; i < colors.Length; i++)
            {
                heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] = (float)(
                    ((colors[i].R / 255.0 - 0.5)*2
                    +
                    1*colors2[i].R / 255.0)/2
            );
                //heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] += 1*;
            }
            Agent.LoadTexture(GraphicsDevice);
            AgentR.LoadTexture(GraphicsDevice);
            AgentB.LoadTexture(GraphicsDevice);

            agents = new List<Agent>();

            int mount = 12;
            for (int i = 0; i < _graphics.PreferredBackBufferWidth; i+= mount)
            {
                for (int j = 0; j < _graphics.PreferredBackBufferHeight; j += mount)
                {

                    Agent ag = new Agent();
                    ag.Position = new Vector2(random.Next(1, MapData.X - 1), random.Next(1, MapData.Y - 1));
                    agents.Add(ag);

                    AgentR agr = new AgentR();
                    agr.Position = new Vector2(random.Next(1, MapData.X - 2) + 1, random.Next(1, MapData.Y - 1));
                    agents.Add(agr);

                    AgentB agb = new AgentB();
                    agb.Position = new Vector2(random.Next(1, MapData.X - 2) , random.Next(1, MapData.Y - 1) + 1);
                    agents.Add(agb);
                }
            }
            //for (int i = 0; i < 300; i++)
            //{
            //    agents[i] = new Agent();
            //    agents[i].Position = new Vector2(random.Next(1, MapData.X - 1), random.Next(1, MapData.Y - 1));
            //}

            OnePixeltexture = new Texture2D(GraphicsDevice, 1, 1);
            OnePixeltexture.SetData(new Color[1] { Color.White });

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        double del = 1;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (del<=0)
            {

                for (int i = 0; i < agents.Count; i++)
                {
                    agents[i].act(heightmap, MapData.X, MapData.Y);
                }
                del = 0.0001;
                OneFrame = true;
            }
            else
            {
                del-=gameTime.ElapsedGameTime.TotalSeconds;
            } 

            base.Update(gameTime);
        }

        bool OneFrame = true;
        RenderTarget2D RenderTarget;
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(new Color(0.8f, 0.8f, 0.8f, 1f));
            if (OneFrame)
            {
                GraphicsDevice.SetRenderTarget(RenderTarget);
                _spriteBatch.Begin();
                //RenderTarget.SetData(colors);
                //_spriteBatch.Draw(OnePixeltexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), new Color(.4f, 0.4f, 0.4f, 0.1f));
                //_spriteBatch.Draw(_texture, new Rectangle(0, 0, RenderTarget.Width, RenderTarget.Height), new Color(.4f, 0.4f, 0.4f, 0.2f*(float)Math.Sin(gameTime.ElapsedGameTime.TotalSeconds)));
                //_spriteBatch.Draw(_texture2, new Rectangle(0, 0, RenderTarget.Width, RenderTarget.Height), new Color(.4f, 0.4f, 0.4f, 0.2f* (float)Math.Sin(gameTime.ElapsedGameTime.TotalSeconds)));

                float coef = 1;// (float)MapData.X/_graphics.PreferredBackBufferWidth;
                for (int i = 0; i < agents.Count; i++)
                {
                    agents[i].Draw(_spriteBatch, coef);
                }
                _spriteBatch.End(); 
                OneFrame = false;
                GraphicsDevice.SetRenderTarget(null);
            }
            _spriteBatch.Begin();
            //_spriteBatch.Draw(_texture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
            _spriteBatch.Draw(RenderTarget, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
            _spriteBatch.End();
            _graphics.PreferredBackBufferFormat = SurfaceFormat.Vector4;
            base.Draw(gameTime);
        }
    }
    public class Agent 
    {
        public static Texture2D Texture2D { get; set; }
        static Color drawColor;
        static int size = 4;
        public static void LoadTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D = new Texture2D(graphicsDevice, size, size);
            Color[] colors = new Color[size * size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if ((i - size / 2f) * (i - size / 2f) + (j - size / 2f) * (j - size / 2f) <= size * size / 4f)
                        colors[i + j * size] = Color.White;
                    else
                        colors[i + j * size] = Color.Transparent;
                }
            }
            Texture2D.SetData(colors);
            //drawColor = new Color(0.5f, 0.1f, 0.5f, 0.1f);
            drawColor = new Color(0.3f, 0.9f, 0.3f, 0.1f);
        }
        public Vector2 Position;
        public virtual void act(double[,] heightmap, int mapWidth, int mapHeight)
        {
            if (CheckAndSetPos(mapWidth, mapHeight))
                return;
            double val = heightmap[(int)Position.X, (int)Position.Y] - 0.5;
            float mlt = 8 * (float)Math.PI;
            Position += new Vector2((float)Math.Cos(val * mlt), (float)Math.Sin(val * mlt)) * size / 4;
            //Position += new Vector2((float)val, (float)val) * size;
        }
        public virtual void Draw(SpriteBatch spriteBatch, float coefPos)
        {
            spriteBatch.Draw(Texture2D, (Position - new Vector2(Texture2D.Width / 2f, Texture2D.Height / 2f)) * coefPos, drawColor);
        }
        public bool CheckAndSetPos(int mapWidth, int mapHeight)
        {
            if ((int)Position.Y > mapHeight - 1)
                Position.Y = 1;
            if ((int)Position.X > mapWidth - 1)
                Position.X = 1;
            if ((int)Position.Y < 0)
                Position.Y = mapHeight - 1;
            if ((int)Position.X < 0)
                Position.X = mapWidth - 1;

            if ((int)Position.X < 0 || (int)Position.X > mapWidth - 1
                || (int)Position.Y < 0 || (int)Position.Y > mapHeight - 1)
                return true;
            return false;
        }
    }
    public class AgentR : Agent
    {
        public static new Texture2D Texture2D { get; set; }
        static Color drawColorR;
        static int size = 5;
        public static new void LoadTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D = new Texture2D(graphicsDevice, size, size);
            Color[] colors = new Color[size * size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if ((i - size / 2f) * (i - size / 2f) + (j - size / 2f) * (j - size / 2f) <= size * size / 4f)
                        colors[i + j * size] = Color.White;
                    else
                        colors[i + j * size] = Color.Transparent;
                }
            }
            Texture2D.SetData(colors);
            drawColorR = new Color(0.2f, 0.2f, 0.6f, 0.0005f);
        }
        public override void act(double[,] heightmap, int mapWidth, int mapHeight)
        {
            if (CheckAndSetPos(mapWidth, mapHeight))
                return;
            double val = heightmap[(int)Position.X, (int)Position.Y] - 0.5;
            float mlt = 4 * (float)Math.PI;
            Position += new Vector2((float)Math.Cos(val * mlt), (float)Math.Sin(val * mlt)) * size / 6f;
            Position += new Vector2((float)Math.Cos(2*val * mlt), (float)Math.Sin(2 * val * mlt)) * size / 6f;
        }
        public override void Draw(SpriteBatch spriteBatch, float coefPos)
        {
            spriteBatch.Draw(Texture2D, (Position - new Vector2(Texture2D.Width / 2f, Texture2D.Height / 2f)) * coefPos, drawColorR);
        }
    }
    public class AgentB : Agent
    { 
        static Color drawColorB;
        static int size = 2;
        public Vector2 accel = new Vector2();
        public static new void LoadTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D = Agent.Texture2D;
            drawColorB = new Color(0.5f, 0.3f, 0.9f, 0.1f);
        }
        public override void act(double[,] heightmap, int mapWidth, int mapHeight)
        {
            if (CheckAndSetPos(mapWidth, mapHeight))
                return;
            double val = heightmap[(int)Position.X, (int)Position.Y] - 0.5;
            double mlt = 16f * (float)Math.PI;
            accel *= 0.95f;
            accel += new Vector2((float)Math.Cos(val * mlt), (float)Math.Sin(val * mlt)) * size / 4f;
            Position += accel / 7;
            //Position += new Vector2((float)val, (float)val) * size;
        }
        public override void Draw(SpriteBatch spriteBatch, float coefPos)
        {
            Color color = new Color(
                (float)Math.Sin(10f/accel.LengthSquared()),
                (float)Math.Cos(2* 10f / accel.LengthSquared()), 
                accel.LengthSquared() / 5f, 
                drawColorB.A);
            spriteBatch.Draw(Texture2D, (Position - new Vector2(Texture2D.Width / 2f, Texture2D.Height / 2f)) * coefPos, color);
        }
    }
}