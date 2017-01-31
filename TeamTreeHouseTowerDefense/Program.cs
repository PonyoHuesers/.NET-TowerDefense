using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    class BasicInvader : Invader
    {
        public BasicInvader(Path path) : base(path) { }
    }

    class ChubbyInvader : Invader
    {
        public override int Health { get; protected set; } = 4;

        public ChubbyInvader(Path path) : base(path) { }  //Passes parameter up to the base class.
    }

    class FastInvader : Invader
    {
        protected override int StepSize { get; } = 2;

        public FastInvader(Path path) : base(path) { }  //Passes parameter up to the base class.
    }

    class Game
    {
        public static void Main()
        {
            Map map = new Map(8, 5);  //Width, Height

            try
            {
                Path path = new Path( 
                    new [] {
                        new MapLocation(0, 2, map),
                        new MapLocation(1, 2, map),
                        new MapLocation(2, 2, map),
                        new MapLocation(3, 2, map),
                        new MapLocation(4, 2, map),
                        new MapLocation(5, 2, map),
                        new MapLocation(6, 2, map),
                        new MapLocation(7, 2, map),
                     }
                );

                IInvader[] invaders =
                {
                    new BasicInvader(path),
                    new ShieldedInvader(path),
                    new FastInvader(path),
                    new ChubbyInvader(path),                    
                    new ResurrectingInvader(path)
                };

                Tower[] towers =
                {
                    new Tower(new MapLocation(7, 3, map)),
                    new Tower(new MapLocation(1, 3, map)),
                    new SniperTower(new MapLocation(3, 3, map)),
                    new PowerfulTower(new MapLocation(5, 3, map))
                };

                Level level = new Level(invaders)
                {
                    Towers = towers
                };
                
                bool playerWon = level.Play();

                Console.WriteLine("Player " +  (playerWon ? "won!" : "lost..."));                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    interface IInvader
    {
        MapLocation Location { get; }

        int Health { get; }

        bool HasScored { get; }

        bool IsNeutralized { get; }

        bool IsActive { get; }

        void Move();

        void DecreaseHealth(int factor);
    }

    abstract class Invader : IInvader
    {
        private readonly Path _path;
        private int _pathStep = 0;
        
        protected virtual int StepSize { get; } = 1;

        public MapLocation Location
        {
            get
            {
                return _path.GetLocationAt(_pathStep);
            }
        }

        public virtual int Health { get; protected set; } = 2;

        public bool HasScored { get { return _pathStep >= _path.Length; } }

        public bool IsNeutralized => Health <= 0;

        public bool IsActive => !(IsNeutralized || HasScored);

        public Invader(Path path)
        {
            _path = path;
        }

        public void Move() => _pathStep += StepSize;

        public virtual void DecreaseHealth(int factor)
        {
            Health -= factor;
            Console.WriteLine("Shot at and hit an invader!");
        }
    }

    class Level
    {
        private readonly IInvader[] _invaders;

        public Tower[] Towers { get; set; }

        public Level(IInvader[] invaders)
        {
            _invaders = invaders;
        }

        //Returns: true if the player wins, false otherwise.
        public bool Play()
        {
            int remainingInvaders = _invaders.Length;

            //Run until all invaders are neutralized or an invader reaches the end of the path.
            while(remainingInvaders > 0)
            {
                //Each tower has opportunity to fire on invaders.
                foreach(Tower tower in Towers)
                {
                    tower.FireOnInvaders(_invaders);
                }
                
                //Recount invaders again.
                remainingInvaders = 0;
                foreach(IInvader invader in _invaders)
                {
                    if (invader.IsActive)
                    {
                        invader.Move();

                        if (invader.HasScored)
                        {
                            return false;
                        }

                        remainingInvaders++;
                    }
                }
            }

            return true;
        }
    }

    class Map
    {
        public readonly int Width;
        public readonly int Height;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public bool OnMap(Point point)
        {
            return point.X >= 0 && point.X < Width &&
                   point.Y >= 0 && point.Y < Height;
        }

    }

    class MapLocation : Point
    {
        public MapLocation(int x, int y, Map map) : base(x, y)
        {
            if (!map.OnMap(this))
            {
                throw new System.Exception(x + "," + y + " is outside the boundaries of the map.");
            }
        }

        //So coordinates, rather than class name, shows for Tower class' invader.Location
        public override string ToString()
        {
            return X + " , " + Y;
        }

        public bool InRangeOf(MapLocation location, int range)
        {
            return DistanceTo(location) <= range;
        }
    }

    class Path
    {
        private readonly MapLocation[] _path;
        public int Length => _path.Length;

        public Path(MapLocation[] path)
        {
            _path = path;
        }

        public MapLocation GetLocationAt(int pathStep)
        {
            return (pathStep < _path.Length) ? _path[pathStep] : null;              
        }
    }

    class Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int DistanceTo(int x, int y)
        {
            return (int)Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2));
        }

        public int DistanceTo(Point point)
        {
            return DistanceTo(point.X, point.Y);
        }
    }   
  
    class PowerfulTower : Tower
    {
        protected override int Power { get; } = 2;

        public PowerfulTower(MapLocation location) : base(location) { }
    }

    static class Random
    {
        private static System.Random _random = new System.Random();

        public static Double NextDouble()
        {
            return _random.NextDouble();
        }
    }

    class ResurrectingInvader : IInvader
    {
        private BasicInvader _incarnation1;
        private BasicInvader _incarnation2;

        public MapLocation Location => _incarnation1.IsNeutralized ? _incarnation2.Location : _incarnation1.Location;

        public int Health => _incarnation1.IsNeutralized ? _incarnation2.Health : _incarnation1.Health;

        public bool HasScored => _incarnation1.HasScored || _incarnation2.HasScored;

        public bool IsNeutralized => _incarnation1.IsNeutralized && _incarnation2.IsNeutralized;

        public bool IsActive => !(IsNeutralized || HasScored);

        public ResurrectingInvader(Path path)
        {
            _incarnation1 = new BasicInvader(path);
            _incarnation2 = new BasicInvader(path);
        }

        public void Move()
        {
            _incarnation1.Move();
            _incarnation2.Move();
        }

        public void DecreaseHealth(int factor)
        {
            if (!_incarnation1.IsNeutralized)
            {
                _incarnation1.DecreaseHealth(factor);
            }
            else
            {
                _incarnation2.DecreaseHealth(factor);
            }
        }
    }

    class ShieldedInvader : Invader
    {
        public ShieldedInvader(Path path) : base(path) { }  //Passes parameter up to the base class.

        public override void DecreaseHealth(int factor)
        {
            if(Random.NextDouble() < .5)
            {
                base.DecreaseHealth(factor);
            }
            else
            {
                Console.WriteLine("Shot at a shielded invader but it sustained no damage.");
            }    
        }
    }

    class SniperTower : Tower
    {
        protected override int Range { get; } = 2;
        protected override double Accuracy { get; } = 1.0;

        public SniperTower(MapLocation location) : base(location) { }
    }

    class Tower
    {
        protected virtual int Range { get; } = 1;
        protected virtual int Power { get; } = 1;
        protected virtual double Accuracy { get; } = 0.75;
        
        private readonly MapLocation _location;

        public Tower(MapLocation location)
        {
            _location = location;
        }

        public bool IsSuccessfulShot()
        {
            return Random.NextDouble() < Accuracy;
        }

        public void FireOnInvaders(IInvader[] invaders)
        {
            foreach(IInvader invader in invaders)
            {
                if (invader.IsActive && _location.InRangeOf(invader.Location, Range))
                {
                    if (IsSuccessfulShot())
                    {
                        invader.DecreaseHealth(Power);

                        if (invader.IsNeutralized)
                        {
                            Console.WriteLine("Neutralized an invader at " + invader.Location + "!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Shot at and MISSED an invader.");
                    }
                    
                    break;
                }
            }
        }

    }

}
