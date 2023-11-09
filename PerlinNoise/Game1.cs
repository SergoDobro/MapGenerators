using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Net.Http.Headers;
using Microsoft.VisualBasic;

namespace PerlinNoise
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        int[,] heightmap;
        Texture2D _texture;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            double km = 0;
            double m = 0;
            double mi = 1;
            for (int i = 0; i < 1000; i++)
            {
                km += PerlinNoise.GetRandom(i);
                //System.Diagnostics.Debug.WriteLine(PerlinNoise.GetRandom(i));
                if (PerlinNoise.GetRandom(i) > m)
                    m = PerlinNoise.GetRandom(i);
                if (PerlinNoise.GetRandom(i) < mi)
                    mi = PerlinNoise.GetRandom(i);
            }
            System.Diagnostics.Debug.WriteLine("res:");
            System.Diagnostics.Debug.WriteLine(km / 1000);
            System.Diagnostics.Debug.WriteLine(m);
            System.Diagnostics.Debug.WriteLine(mi);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        float avLevelH = 0.9f;
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _texture = new Texture2D(GraphicsDevice, 500, 500);
            PerlinNoise.SetTexture(GraphicsDevice, _texture, 500, 500);

            _graphics.PreferredBackBufferHeight = _texture.Height;
            _graphics.PreferredBackBufferWidth = _texture.Width;
            _graphics.ApplyChanges();

            return;
            heightmap = new int[_texture.Width, _texture.Height];
            Color[] colors = new Color[_texture.Width * _texture.Height];
            _texture.GetData<Color>(colors);
            for (int i = 0; i < colors.Length; i++)
            {
                heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] = (int)colors[i].R;
            }
            CountAv();


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

            WorkWithAgents(colors);

            _texture.SetData<Color>(colors);
        }
        float average;
        public void CountAv()
        {
            int c = 0;
            for (int i = 0; i < _texture.Width * _texture.Height; i++)
            {
                if (heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] > 8)
                {
                    average += heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width];
                    c++;
                }
                else
                {
                    if (heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] > 0)
                    {
                        //System.Diagnostics.Debug.WriteLine(heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width]);
                    }
                }
            }
            average /= c;
        }
        public Point iToPoint(int i) => new Point(i % _texture.Width, (i - i % _texture.Width) / _texture.Width);
        public void WorkWithAgents(Color[] colors)
        {
            List<Point> points = new List<Point>();
            List<Point> directions = new List<Point>();
            Random random = new Random();
            for (int i = 0; i < _texture.Width * _texture.Height; i++)
            {
                if (heightmap[i % _texture.Width, (i - i % _texture.Width) / _texture.Width] > average * avLevelH
                    /*&& points.Count < 200*/ && random.NextDouble() < 0.001)
                {
                    colors[i].R = 255;
                    points.Add(iToPoint(i));
                    directions.Add(new Point(1, 0));
                }
            }
            for (int iii = 0; iii < 2000; iii++)
            {

                for (int i = 0; i < points.Count; i++)
                {
                    Point _point = points[i];

                    int mn = 3;
                    Tuple<Point, int>[] pointpairs = new Tuple<Point, int>[(2 * mn + 1) * (2 * mn + 1)];

                    for (int k = -mn; k < mn + 1; k++)
                    {
                        for (int kk = -mn; kk < mn + 1; kk++)
                        {
                            if (_point.X + k > 0 && _point.X + k < _texture.Width &&
                                _point.Y + kk > 0 && _point.Y + kk < _texture.Height &&
                                _point.X + k + (_point.Y + kk) * _texture.Width > 0 &&
                                _point.X + k + (_point.Y + kk) * _texture.Width < _texture.Width * _texture.Height
                                && !(k == 0 && k == 0))
                            {
                                if (heightmap[_point.X + k, _point.Y + kk] == 1)
                                {
                                    pointpairs[(k + mn) * (2 * mn + 1) + kk + mn] = new Tuple<Point, int>(new Point(_point.X + k, _point.Y + kk),
                                    10000);
                                }
                                else
                                {
                                    pointpairs[(k + mn) * (2 * mn + 1) + kk + mn] = new Tuple<Point, int>(new Point(_point.X + k, _point.Y + kk),
                                    heightmap[_point.X + k, _point.Y + kk]);
                                }
                            }
                            else
                            {
                                pointpairs[(k + mn) * (2 * mn + 1) + kk + mn] = new Tuple<Point, int>(new Point(0, 0), 10000);
                            }
                        }
                    }
                    pointpairs = pointpairs.OrderBy(x => x.Item2).ToArray();
                    Tuple<Point, int> next;
                    int l = 0;
                    for (int ii = 0; ii < pointpairs.Length; ii++)
                        if (pointpairs[0].Item2 == pointpairs[ii].Item2)
                            l++;
                    if (l > 0)
                    {
                        next = pointpairs[random.Next(0, l)];
                    }
                    else
                    {
                        next = pointpairs[0];
                    }
                    points[i] = next.Item1;
                    int s = 0;
                    for (int k = -s; k < s + 1; k++)
                    {
                        for (int kk = -s; kk < s + 1; kk++)
                        {
                            if (_point.X + k + (_point.Y + kk) * _texture.Width > 0 &&
                                _point.X + k + (_point.Y + kk) * _texture.Width < _texture.Width * _texture.Height)
                            {
                                if (next.Item1.X >= 0 &&
                                    next.Item1.X < _texture.Width &&
                                    next.Item1.Y >= 0 &&
                                    next.Item1.Y < _texture.Height)
                                {

                                    colors[_point.X + k + (_point.Y + kk) * _texture.Width].R = (byte)(heightmap[_point.X, _point.Y] / 2);// (byte)(heightmap[next.Item1.X, next.Item1.Y]-1);//(byte)iii; //(byte)heightmap[_point.X, _point.Y];
                                    colors[_point.X + k + (_point.Y + kk) * _texture.Width].G = (byte)(heightmap[_point.X, _point.Y] / 2);//(byte)(heightmap[next.Item1.X, next.Item1.Y]-1);//(byte)iii; //(byte)heightmap[_point.X, _point.Y];
                                    colors[_point.X + k + (_point.Y + kk) * _texture.Width].B = (byte)(125 - heightmap[_point.X, _point.Y] / 2);//(byte)(255-(Math.Sin(iii/1000f)+1)/2*255);// (byte)heightmap[_point.X, _point.Y];
                                }
                            }
                        }
                    }

                    if (random.NextDouble() < 0.0001)
                    {
                        points.Add(points[i]);
                        directions.Add(directions[i]);
                    }
                    if (_point.X >= 0 &&
                        _point.X < _texture.Width &&
                        _point.Y >= 0 &&
                        _point.Y < _texture.Height)
                    {
                        if (heightmap[_point.X, _point.Y] < 1)
                        {
                            points.RemoveAt(i);
                            directions.RemoveAt(i);
                            i--;
                        }
                        else
                            heightmap[_point.X, _point.Y] = Math.Max(1, heightmap[_point.X, _point.Y] - 10);// (heightmap[_point.X, _point.Y])/2;
                    }
                    else
                    {

                    }
                }
            }
        }

        double del = 0;
        int m = 0;
        float lastlayerdelta = 0;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (del < 0)
            {
                //m = (m + 1) % 10;
                lastlayerdelta += 2f;
                PerlinNoise.SetTexture(GraphicsDevice, _texture, 500, 500, 5, lastlayerdelta);
                del = 1f/gameTime.ElapsedGameTime.TotalSeconds;
                del /= 25f;
            }
            del--;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            int m = 1;
            _spriteBatch.Draw(_texture, new Rectangle(-(int)(_texture.Width * (m - 1) / 2), -(int)(_texture.Height * (m - 1) / 2), (int)(_texture.Width * m), (int)(_texture.Height * m)), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
    public class PerlinNoise
    {
        public static int dseed = 0;
        public static double GetRandom(int _seed, int dSeed = 0x6D2B79F8)
        {
            const double randMax = 4294967296;
            //_seed = 214013 * _seed + 2531011;
            //_seed ^= _seed >> 15;
            //return (_seed / randMax); 
           _seed += ((dSeed+dseed) * 73129 + 95121) % 100000;
            var t = (_seed ^ (_seed >> 15)) * (1 | _seed);
            t = (t + ((t ^ (t >> 7)) * (61 | t))) ^ t;
            return (((t ^ (t >> 14)) % randMax) / randMax - 0.25) * 4; // /2^32
        }
        public Vector2 GetVector(int x, int y)
        { 
            return new Vector2(x, y);
        } 
        static Color[] colors = new Color[1];
        public static void SetTexture(GraphicsDevice graphicsDevice, Texture2D texture, int w = 1000, int h = 1000, int octaves = 3, float lastlayerdelta = 0, int scale = 256)
        {
            if (h * w != colors.Length)
            {
                colors = new Color[w * h];
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        colors[i + j * w] = new Color(0, 0, 0);
                    }
                }
            }

            double[] pows = new double[octaves];
            for (int i = 0; i < octaves; i++)
            {
                pows[i] = Math.Pow(2, i);
            }
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    double val = 0;
                    for (int ii = 1; ii <= octaves - 1; ii++)
                    {
                        val += GetValue(i, j, (int)(scale / (pows[ii]))) / pows[ii - 1];
                    }
                    for (int ii = 0; ii < 1; ii++)
                    {
                        val += 2 * GetValue(i + lastlayerdelta, j, (int)(scale / (octaves))) / pows[octaves - 1];
                    }
                    //val *= 6;
                    //val *= val * Math.Sign(val);
                    //val *= val * Math.Sign(val);
                    //val  = Math.Sign(val)*Math.Pow(val, 3/3f);
                    //val *= 5;
                    //val = Math.Round(val);
                    //val /= 5f;
                    byte hh = (byte)((val + 1) * 125);//(int) (255*GetRandom((int)(125 * (GetRandom(i / mult) + GetRandom(j /mult, 0x6AFB7BF1) + 2)/2f)));
                    colors[i + j * w].R = hh;
                    colors[i + j * w].G = hh;
                    colors[i + j * w].B = hh;
                }
            }
            texture.SetData(colors);
        }
        public static void ScaleTall_Texture(Texture2D texture, Func<int, int> scaler)
        {
            int w = texture.Width;
            colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);
            for (int i = 0; i < texture.Width; i++)
            {
                for (int j = 0; j < texture.Height; j++)
                {
                    colors[i + j * w].R = (byte)scaler((int)colors[i + j * w].R);
                    colors[i + j * w].G = colors[i + j * w].R;
                    colors[i + j * w].B = colors[i + j * w].R;
                }
            }
            texture.SetData(colors);
        }
        static Vector2 dirTL;
        static Vector2 dirTR;
        static Vector2 dirBL;
        static Vector2 dirBR;
        public static double GetValue(float x, float y, int mult)
        {
            x /= mult * 1f;
            y /= mult * 1f; 
            Vector2 v = new Vector2(x, y);

            dirTL.X = (int)Math.Floor(x);
            dirTL.Y = (int)Math.Floor(y);

            dirTR.X = dirTL.X + 1;
            dirTR.Y = dirTL.Y;

            dirBL.X = dirTL.X;
            dirBL.Y = dirTL.Y + 1;

            dirBR.X = dirTL.X + 1;
            dirBR.Y = dirTL.Y + 1; 

            var dot1 = GetDotFromVecs(v, dirTL);
            var dot2 = GetDotFromVecs(v, dirTR);
            var dot3 = GetDotFromVecs(v, dirBL);
            var dot4 = GetDotFromVecs(v, dirBR); 
            Vector2 sv = new Vector2(Interpolate(x, dirTL.X), Interpolate(y, dirTL.Y));
            var rx = dot1 + (dot2 - dot1) * sv.X;
            var rx2 = dot3 + (dot4 - dot3) * sv.X;
            var ry = rx + (rx2 - rx)*sv.Y;
            if (ry is double.NaN) return 0;
            return ry;
        }
        /*
         
            Sx = 3(x - x0)² - 2(x - x0)³
            a = s + Sx(t - s)
            b = u + Sx(v - u)
         */
        public static float Interpolate(float x, float x0)
        {
            return -(x0 - x);// (float)(3 * Math.Pow(x - x0, 2) - 2 * Math.Pow(x - x0, 3));
        }
        public static double GetDotFromVecs(Vector2 vectorA, Vector2 vectorB)
        {
            var dir = GetDirInt(vectorB.X, vectorB.Y);
            var kkk = vectorA - vectorB; 
            var dot1 = dotProduct(dir, kkk); 
            return dot1;
        }
        public static Vector2 GetDirInt(float x, float y)
        {
            //var kk = GetRandom((int)(125 * (GetRandom(x / mult) + GetRandom(y / mult, 0x6AFB7BF1) + 2) / 2f));
            var mm = (250 * GetRandom((int)(y + 1000 * x)));
            var a =  new Vector2(
                (float)(Math.Cos(mm)), 
                (float)Math.Sin(mm)
                ); 
            return a;// new Vector2(-0.5f,0.5f);
        }
        public static double dotProduct(Vector2 vectorA, Vector2 vectorB)
        {
            return vectorA.X*vectorB.X + vectorA.Y*vectorB.Y;
        }
    }
}