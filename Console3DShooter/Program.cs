using System;
using System.Collections.Generic;
using System.Linq;

namespace Console3DShooter
{
    class Program
    {
        private const int ScreenWidth = 100;
        private const int ScreenHeight = 50;

        private const int MapWidth = 32;
        private const int MapHeight = 32;

        private const double Fov = Math.PI / 3;
        private const double Depth = 16;

        private static double _playerX = 5;
        private static double _playerY = 5;
        private static double _playerA = 0;

        private static string _map = "";

        private static readonly char[] Screen = new char[ScreenWidth * ScreenHeight];
        static void Main(string[] args)
        {
            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
            Console.CursorVisible = false;

            _map += "################################";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#......##########..............#";
            _map += "#...............#..............#";
            _map += "#..................##..........#";
            _map += "#...............#..............#";
            _map += "#......##########..............#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..........######..............#";
            _map += "#..........#...................#";
            _map += "#..........#..#######..........#";
            _map += "#..........#...................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#..............................#";
            _map += "#.............############.....#";
            _map += "#.............#................#";
            _map += "#.............#................#";
            _map += "#.............#................#";
            _map += "#.............#................#";
            _map += "#.............#................#";
            _map += "#.............#................#";
            _map += "################################";

            DateTime dateTimeFrom = DateTime.Now;

            float rotSpeed = 5;
            float moveSpeed = 30;

            while (true)
            {

                DateTime dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                if (Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;

                    switch (consoleKey)
                    {
                        case ConsoleKey.A:
                            _playerA += rotSpeed * elapsedTime;
                            break;
                        case ConsoleKey.D:
                            _playerA -= rotSpeed * elapsedTime;
                            break;
                        case ConsoleKey.W:
                            {
                                _playerX += Math.Sin(_playerA) * moveSpeed * elapsedTime;
                                _playerY += Math.Cos(_playerA) * moveSpeed * elapsedTime;

                                if (_map[(int) _playerY * MapWidth + (int) _playerX] == '#')
                                {
                                    _playerX -= Math.Sin(_playerA) * moveSpeed * elapsedTime;
                                    _playerY -= Math.Cos(_playerA) * moveSpeed * elapsedTime;
                                }

                                break;
                            }
                            
                        case ConsoleKey.S:
                            {
                                _playerX -= Math.Sin(_playerA) * moveSpeed * elapsedTime;
                                _playerY -= Math.Cos(_playerA) * moveSpeed * elapsedTime;

                                if (_map[(int)_playerY * MapWidth + (int)_playerX] == '#')
                                {
                                    _playerX += Math.Sin(_playerA) * moveSpeed * elapsedTime;
                                    _playerY += Math.Cos(_playerA) * moveSpeed * elapsedTime;
                                }

                                break;
                            }
                            
                    }
                }
                
                for (int x = 0; x < ScreenWidth; x++)
                {
                    double rayAngle = _playerA + Fov / 2 - x * Fov / ScreenWidth;

                    double rayX = Math.Sin(rayAngle);
                    double rayY = Math.Cos(rayAngle);

                    double distanceToWall = 0;
                    bool hitWall = false;
                    bool isBound = false;

                    while (!hitWall && distanceToWall < Depth)
                    {
                        distanceToWall += 0.1;

                        int testX = (int)(_playerX + rayX * distanceToWall);
                        int testY = (int)(_playerY + rayY * distanceToWall);

                        if (testX < 0 || testX >= Depth + _playerX || testY < 0 || testY >= Depth + _playerY)
                        {
                            hitWall = true;
                            distanceToWall = Depth;
                        }
                        else
                        {
                            char testCell = _map[testY * MapWidth + testX];

                            if (testCell == '#')
                            {
                                hitWall = true;

                                var boundsVectorList = new List<(double module, double cos)>();

                                for (int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; ty < 2; ty++)
                                    {
                                        double vx = testX + tx - _playerX;
                                        double vy = testY + ty - _playerY;

                                        double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        double cosAngle = rayX * vx / vectorModule + rayY * vy / vectorModule;

                                        boundsVectorList.Add((vectorModule, cosAngle));

                                    }
                                }

                                boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

                                double boundAngle = 0.03 / distanceToWall;

                                if(Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                                   Math.Acos(boundsVectorList[1].cos) < boundAngle)
                                {
                                    isBound = true;
                                }

                            }
                        }

                    }

                    int ceiling = (int)(ScreenHeight / 2d - ScreenHeight * Fov / distanceToWall);
                    int floor = ScreenHeight - ceiling;

                    char wallShade;

                    if (isBound)
                        wallShade = '|';                 
                    else if (distanceToWall <= Depth / 4d)
                        wallShade = '\u2588';
                    else if (distanceToWall < Depth / 3d)
                        wallShade = '\u2593';
                    else if (distanceToWall < Depth / 2d)
                        wallShade = '\u2592';
                    else if (distanceToWall < Depth)
                        wallShade = '\u2591';
                    else
                        wallShade = ' ';

                    for (int y = 0; y < ScreenHeight; y++)
                    {
                        if (y <= ceiling)
                        {
                            Screen[y * ScreenWidth + x] = ' ';
                        }
                        else if (y > ceiling && y <= floor)
                        {
                            Screen[y * ScreenWidth + x] = wallShade;
                        }
                        else
                        {
                            char floorShade;

                            double b = 1 - (y - ScreenHeight / 2d) / (ScreenHeight / 2d);

                            if (b < 0.25)
                                floorShade = '#';
                            else if (b < 0.5)
                                floorShade = 'x';
                            else if (b < 0.75)
                                floorShade = '-';
                            else if (b < 0.9)
                                floorShade = '.';
                            else
                                floorShade = ' ';

                            Screen[y * ScreenWidth + x] = floorShade;
                        }
                    }

                }


                //stats
                char[] stats = $"X: {_playerX}, Y: {_playerY}, A: {_playerA}, FPS: {(int)(1 / elapsedTime)}"
                    .ToCharArray();
                stats.CopyTo(Screen, 0);

                //map
                for (int x = 0; x < MapWidth; x++)
                {
                    for (int y = 0; y < MapHeight; y++)
                    {
                        Screen[(y + 1) * ScreenWidth + x] = _map[y * MapWidth + x];
                    }
                }
                //player
                Screen[(int)(_playerY + 1) * ScreenWidth + (int)_playerX] = 'P';



                Console.SetCursorPosition(0, 0);
                Console.Write(Screen);

            }

        }
    }
}
