using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PerlinNoise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 

namespace MapGenerators
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        double[,] heightmap;
        Texture2D _texture;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }
        const int sc = 3;
        float avLevelH = 0.9f;
        Random random = new Random();
        Point MapData = new Point((int)(50 * Math.Pow(2, sc)), (int)(50 * Math.Pow(2, sc)));
        Color[] colors;
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //_texture = Texture2D.FromFile(GraphicsDevice, "heightmap.bmp");
            _texture = new Texture2D(GraphicsDevice, MapData.X, MapData.Y);

            PerlinNoise.PerlinNoise.SetTexture(null, _texture, MapData.X, MapData.Y, 16, scale: 32*(int)Math.Pow(2, sc));
            //PerlinNoise.PerlinNoise.ScaleTall_Texture(_texture, (a)=>(byte)(255*Math.Clamp(Math.Pow(0.3+a/255f,6),0,1)));

            _graphics.PreferredBackBufferHeight = 1000;// _texture.Height;
            _graphics.PreferredBackBufferWidth = 1000;//_texture.Width;
            _graphics.ApplyChanges();

            heightmap = new double[_texture.Width, _texture.Height];
            colors = new Color[_texture.Width * _texture.Height];
            _texture.GetData<Color>(colors);
            for (int i = 0; i < colors.Length; i++)
            {
                heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] = colors[i].R/255.0;
            } 
            


            for (int i = 0; i < colors.Length; i++)
            {
                if ((int)colors[i].R > average * avLevelH)
                {
                    //colors[i].R = 255;
                }
                if ((int)colors[i].R > 0 && (int)colors[i].R < 9)
                {
                    colors[i].R = 0;
                    colors[i].G = 0;
                    colors[i].B = 0;
                }
            } 

            UpdateMap();
        }
        double average;
        public void CountAv()
        {
            int c = 0;
            for (int i = 0; i < _texture.Width * _texture.Height; i++)
            {
                if (heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] > 0.6)
                {
                    average += heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width];
                    c++;
                }
            }
            average /= c;
        }
        public int CountR(double a, double b)
        {
            int c = 0;
            for (int i = 0; i < _texture.Width * _texture.Height; i++)
            {
                if (heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] > a
                    &&
                    heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] < b)
                { 
                    c++;
                }
            }
            return c;
        }
        public Point iToPoint(int i) => new Point(i % _texture.Width, (i - i % _texture.Width) / _texture.Width);
        Spill[] CreateSpills()
        {
            double times = sc*3;
            CountAv();
            //int m = CountR(0.7, 1);
            List<Spill> spills = new List<Spill>();
            for (int i = 0; i < _texture.Width; i++)
            {
                for (int j = 0; j < _texture.Height; j++)
                {
                    if (heightmap[i, j] > 0.6)
                    {
                        if (random.NextDouble() < 0.0001 * times)
                        {
                            spills.Add(new Spill(new Point(i, j), heightmap));
                        }
                    }
                    else
                    {
                        if (random.NextDouble() < 0.00001 * times)
                        {
                            spills.Add(new Spill(new Point(i, j), heightmap));
                        }
                    }
                }
            } 
            return spills.ToArray(); 
        }
        public void UpdateMap()
        {

            for (int i = 0; i < colors.Length; i++)
            {

                var h = (byte)(255 * heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width]);
                colors[i].R = (byte)(h);
                colors[i].G = (byte)(h);
                colors[i].B = (byte)(h);
            }
            _texture.SetData<Color>(colors);
        }
        public void UpdateMap_savewater()
        {

            for (int i = 0; i < colors.Length; i++)
            {
                //var h = (byte)(255 * heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width]);
                //colors[i].R = (byte)(h);
                //colors[i].G = (byte)(h);
            }
            _texture.SetData<Color>(colors);
        }
        KeyValuePair<double, Color>[] palete = (new Dictionary<double, Color>() {
            { 0.0, Color.Black },
            { 0.02, Color.DarkBlue },
            { 0.09, Color.Blue },
            { 0.1, Color.Yellow },
            { 0.3, Color.LightGreen },
            { 0.4, Color.Green },
            { 0.5, Color.Gray },
            { 0.8, Color.LightGray }, 
            { 1, Color.White },
            { 2, Color.Orange },
        }).ToArray();
        public void UpdateMap_Color()
        {

            for (int i = 0; i < colors.Length; i++)
            {
                var h = (heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width]);
                for (int uu = 0; uu < palete.Length - 2; uu++)
                {
                    if (palete[uu].Key <= h && h <= palete[uu + 1].Key)
                    {
                        colors[i] = palete[uu + 1].Value;
                        colors[i].R = (byte)(colors[i].R * (0.2 + h));
                        colors[i].G = (byte)(colors[i].G * (0.2 + h));
                        colors[i].B = (byte)(colors[i].B * (0.2 + h));
                    }
                }
            }
            _texture.SetData<Color>(colors);
        }
        public void UpdateMap_Color_R()
        {
            double a = 0.6;
            double b = 5;

            for (int i = 0; i < _texture.Width; i++)
            {
                for (int j = 0; j < _texture.Height; j++)
                {
                    if (heightmap[i, j] > a && heightmap[i, j] < b)
                    {
                        colors[i+j*_texture.Width] = Color.Red;
                    }
                }
            } 
            _texture.SetData<Color>(colors);
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            if (Keyboard.GetState().IsKeyDown(Keys.D1))
                UpdateMap_savewater();
            if (Keyboard.GetState().IsKeyDown(Keys.D2))
                UpdateMap();
            if (Keyboard.GetState().IsKeyDown(Keys.D3))
                UpdateMap_Color();
            if (Keyboard.GetState().IsKeyDown(Keys.D4))
                UpdateMap_Color_R();
            for (int j = 0; j < 1+15/sc; j++)
            { 
                Spill[] spills = CreateSpills();// new Spill[spilCount];
                int spilCount = spills.Length;
                //for (int i = 0; i < spilCount; i++)
                //{
                //    spills[i] = new Spill(new Point(random.Next(1, MapData.X-1), random.Next(1, MapData.Y - 1)), heightmap);
                //}
                for (int i = 0; i < 2000; i++)
                {
                    for (int ii = 0; ii < spilCount; ii++)
                    {
                        var cl = spills[ii].Move(MapData.X, MapData.Y, heightmap);
                        colors[cl.X + cl.Y * MapData.X].B = (byte)(Math.Clamp(colors[cl.X + cl.Y * MapData.X].B + 1,0,255));
                        //colors[cl.X + cl.Y * MapData.X].R = 0;
                        //colors[cl.X + cl.Y * MapData.X].B +=1;
                        //colors[cl.X + cl.Y*MapData.X].R = 0;4
                    } 
                }
                for (int i = 0; i < spilCount; i++)
                {
                    spills[i].Release(MapData.X, MapData.Y, heightmap);
                }
                
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                using (StreamWriter wr = new StreamWriter("map.png"))
                {
                    _texture.SaveAsPng(wr.BaseStream, MapData.X, MapData.Y);
                }
                System.Threading.Thread.Sleep(400);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            int m = 1;
            //_spriteBatch.Draw(_texture, new Rectangle(-(int)(_texture.Width * (m - 1) / 2), -(int)(_texture.Height * (m - 1) / 2), (int)(_texture.Width * m), (int)(_texture.Height * m)), Color.White);
            _spriteBatch.Draw(_texture, new Rectangle(0,0, (int)_graphics.PreferredBackBufferWidth, (int)_graphics.PreferredBackBufferHeight), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
    class Spill
    {
        Point position;
        public double curHeight = 0;
        public static Random Random = new Random();
        double accumulated = 0.001;
        public Vector2 direction = new Vector2();

        public Spill(Point position, double[,] heightmap)
        {
            this.position = position;
            this.curHeight = heightmap[position.X, position.Y];
        }
        public Point Move(int width, int height, double[,] heightmap)
        {
            if (curHeight <= 0.1)
            {
                Release(width, height, heightmap);
                return position;
            }
            Point new_position = position;// + new Point((int)(Math.Sign(direction.X), (int)(Math.Sign(direction.Y));
            ;
            double bord = 0.5;
            if (direction.Y / direction.Length() > bord)
                new_position.Y += 1;
            if (direction.Y / direction.Length() < -bord)
                new_position.Y += -1;
            if (direction.X / direction.Length() > bord)
                new_position.X += 1;
            if (direction.X / direction.Length() < -bord)
                new_position.X += -1;
            // new_position += new Point((int)Math.Clamp(direction.X, -1, 1), (int)(Math.Clamp(direction.Y, -1, 1)));;

            if (new_position.X < 0 || new_position.X >= width ||
                new_position.Y < 0 || new_position.Y >= height)
                return position; 
            double next_h = heightmap[new_position.X, new_position.Y];

            double a = 0.1;
            direction = direction*0.1f+1*Calculate_Enercia(width, height, heightmap);

            double coef = 0.004;
            double coef2 = 0.002;
            double coefEvaporation = 0.01;


            if (position == new_position)
            {
                double am2 = (coef2 * (accumulated) * Math.Sqrt(direction.Length()));
                NearSpill(width, height, heightmap, am2, new_position);
                //heightmap[new_position.X, new_position.Y] += am2;
                accumulated -= am2;
            }
            else
            {
                if (curHeight < next_h || accumulated > 0.25 / (5 * direction.Length() + 1))
                {
                    double am2 = (coef2 * (accumulated));
                    NearSpill(width, height, heightmap, am2, new_position);
                    //heightmap[new_position.X, new_position.Y] += am2;
                    accumulated -= am2; 
                }
                else if (next_h > 0)
                {
                    double am = (coef * (curHeight - next_h) * Math.Sqrt(direction.Length()));
                    accumulated += am;
                   // NearSpill(width, height, heightmap, -am, position);
                    heightmap[position.X, position.Y] -= am;
                }
            }

            if (accumulated < 0)
            {

            }
            if (next_h < 0)
            {

            }

            position = new_position;
            curHeight = heightmap[new_position.X, new_position.Y];
            accumulated *= 1-coefEvaporation;

            //if (Math.Sign(direction.X) == 0 && Math.Sign(direction.X) == Math.Sign(direction.Y))
            //{
            //    //Release(width, height, heightmap);
            //}
            return position;
        }
        public bool Release(int width, int height, double[,] heightmap)
        {
            //heightmap[position.X, position.Y] += ( (accumulated));
            NearSpill(width, height, heightmap, accumulated, position);
            accumulated = 0;
            return true;
        }
        public Vector2 Calculate_Enercia(int width, int height, double[,] heightmap)
        {
            Vector2 direction = new Vector2(0, 0);
            int n = 0;
            for (int k = -1; k < 1 + 1; k++)
            {
                for (int kk = -1; kk < 1 + 1; kk++)
                {
                    if (position.X + k > 0 && position.X + k < width &&
                        position.Y + kk > 0 && position.Y + kk < height
                        && !(k == 0 && k == 0))
                    {
                        double hh = heightmap[position.X + k, position.Y + kk];
                        if (kk == k)
                        {
                            direction += (float)Math.Pow(hh - curHeight, 3) * new Vector2(k, kk) / (float)Math.Sqrt(2);// * (float)(accumulated+1)/10.0f;
                        }
                        else
                        {
                            direction += (float)Math.Pow(hh - curHeight, 3) * new Vector2(k, kk);// * (float)(accumulated+1)/10.0f;
                        }
                        n++;
                    }
                    else
                    {
                    }
                }
            }
            direction /= n;
            return -(direction / (float)accumulated) * 1f;
        }
        public void NearSpill(int width, int height, double[,] heightmap, double spill_acc, Point position)
        {
            Vector2 direction = new Vector2(0, 0);
            int n = 0;

            int kkk = 0;
            for (int k = -1; k < 1 + 1; k++)
            {
                for (int kk = -1; kk < 1 + 1; kk++)
                {
                    if (position.X + k > 0 && position.X + k < width &&
                        position.Y + kk > 0 && position.Y + kk < height)
                    { 
                        n++;
                    }
                    else
                    {
                    }
                }
            }
            spill_acc /= (n + kkk); 
            for (int k = -1; k < 1 + 1; k++)
            {
                for (int kk = -1; kk < 1 + 1; kk++)
                {
                    if (position.X + k > 0 && position.X + k < width &&
                        position.Y + kk > 0 && position.Y + kk < height)
                    {
                        heightmap[position.X + k, position.Y + kk] += spill_acc;
                    }
                    else
                    {
                    }
                }
            }
            heightmap[position.X  , position.Y  ] += spill_acc* kkk;

        }
    }
}