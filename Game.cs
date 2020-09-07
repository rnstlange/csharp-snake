using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snk
{

    public struct Coordinate
    {
        public int X; // Horizontal
        public int Y; // Vertical

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return !c1.Equals(c2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Apple
    {
        private readonly Random _random;
        public Coordinate Coordinate;

        private readonly int _width;

        // private int _height;
        private readonly int _fields;

        public Apple(Random random, int width, int height)
        {
            _random = random;
            _width = width;
            // _height = height;
            _fields = width * height;
        }

        public void place(Coordinate[] snake)
        {
            var usedCoordinates = snake.Select(c => c.X * c.Y).ToArray();
            var coordinate = _random.Next(_fields - usedCoordinates.Length);
            while (usedCoordinates.Contains(coordinate))
            {
                coordinate++;
            }

            var y = coordinate / _width;
            var x = coordinate % _width;

            Coordinate = new Coordinate {X = x, Y = y};
        }
    }

    public enum Direction
    {
        Left, // X + 1
        Right, // X - 1
        Up, // Y - 1
        Down // Y + 1
    }

    public class Snake
    {
        public Direction LastDirection = Direction.Up;
        public Direction Direction = Direction.Up;
        public Coordinate Head;
        public Coordinate[] Tail;

        public Coordinate[] Body => Tail.Append(Head).ToArray();

        public Snake(int x, int y)
        {
            this.Head = new Coordinate {X = x, Y = y};
            this.Tail = new Coordinate[] {};
        }
        
        // 1 оценка - сделать +
        // 2 оценка - доработать
        // 3 оценка - сделать меню, выбор режима, змейка откусывает хвост

        public void Move()
        {
            var tmp = Head;

            switch (Direction)
            {
                case Direction.Left:
                {
                    Head.X--;
                    break;
                }
                case Direction.Right:
                {
                    Head.X++;
                    break;
                }
                case Direction.Up:
                {
                    Head.Y--;
                    break;
                }
                case Direction.Down:
                {
                    Head.Y++;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Tail.Length > 1 && Array.FindIndex(Tail, el => el == Head) >= 0)
                throw new Exception("GAME OVER"); // TODO: Remove exception

            /*
            if (Head == apple.Coordinate)
            {
                Body = Body.Prepend(tmp).ToArray();
                apple.place(Tail.Append(Head).ToArray());
                return;
            }
            */

            if (Tail.Length > 0)
            {
                Tail = Tail.Take(Tail.Length - 1).Prepend(tmp).ToArray();
            }
        }
    }

    public enum ViewObj
    {
        Wall = '#',
        SnakeHead = 'x',
        SnakeBody = '*',
        Apple = '@',
    }

    public class Writer
    {
        private readonly int origRow;
        private readonly int origCol;

        public Writer(int row = 0, int col = 0)
        {
            origRow = Console.CursorTop + row;
            origCol = Console.CursorLeft + col;
        }

        public void Write(string s, int x, int y)
        {
            Console.SetCursorPosition(origCol + x, origRow + y);
            Console.Write(s);
        }

        public void CursorToTheEnd()
        {
            Console.SetCursorPosition(origRow, origCol);
        }
    }

    public class Game
    {
        private const double Fps = 4;
        private readonly Writer _writer;
        private readonly int _width;
        private readonly int _height;
        private readonly Snake _snake;
        private readonly Apple _apple;

        public Game(int width = 40, int height = 20)
        {
            _writer = new Writer(1);
            var random = new Random();
            _width = width;
            _height = height;
            _snake = new Snake(width / 2, height / 2);
            _apple = new Apple(random, _width, _height);
            _apple.place(new[] {_snake.Head});

            Task.Run(() =>
            {
                while (true)
                    if (Console.KeyAvailable)
                        _snake.Direction = Console.ReadKey().Key switch
                        {
                            ConsoleKey.W => Direction.Up,
                            ConsoleKey.A => Direction.Left,
                            ConsoleKey.S => Direction.Down,
                            ConsoleKey.D => Direction.Right,
                            _ => _snake.Direction
                        };
            });

            // Console.SetWindowSize(height, width);
            // Console.SetBufferSize(height, width);
        }

        public void Loop()
        {
            while (true)
            {
                if ((_snake.Head.Y == 0 && _snake.Direction == Direction.Up) ||
                    (_snake.Head.Y == _height - 1 && _snake.Direction == Direction.Down) ||
                    (_snake.Head.X == 0 && _snake.Direction == Direction.Left) ||
                    (_snake.Head.X == _width - 1 && _snake.Direction == Direction.Right))
                {
                    break;
                }

                Console.Clear();

                var tmp = _snake.Tail.Length > 0 ? _snake.Tail.Last() : _snake.Head;
                _snake.Move();
                if (_snake.Head == _apple.Coordinate)
                {
                    _snake.Tail = _snake.Tail.Append(tmp).ToArray();
                    _apple.place(_snake.Body);
                }

                Console.WriteLine("Score: " + _snake.Tail.Length);

                View();
                _writer.CursorToTheEnd();
                Thread.Sleep((int) (1000.0 / Fps));
            }
        }

        public void View()
        {
            var width = _width + 2;
            var height = _height + 2;

            _writer.Write(new string((char) ViewObj.Wall, width), 0, 0);
            _writer.Write(new string((char) ViewObj.Wall, width), 0, height - 1);

            for (var i = 0; i < height; i++)
            {
                _writer.Write(((char) ViewObj.Wall).ToString(), 0, i);
                _writer.Write(((char) ViewObj.Wall).ToString(), width - 1, i);
            }

            _writer.Write(((char) ViewObj.SnakeHead).ToString(), _snake.Head.X + 1, _snake.Head.Y + 1);

            foreach (var c in _snake.Tail)
                _writer.Write(((char) ViewObj.SnakeBody).ToString(), c.X + 1, c.Y + 1);

            _writer.Write(((char) ViewObj.Apple).ToString(), _apple.Coordinate.X + 1, _apple.Coordinate.Y + 1);
        }
    }
}