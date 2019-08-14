using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlakeServerSide.Data
{
    public class SnakeService
    {
        public const int BoxSize = 24;
        public const int MapSize = 20;

        public SnakeTile[,] Screen { get; set; } = new SnakeTile[MapSize, MapSize];
        public SnakeGameState CurrentState { get; set; } = SnakeGameState.Stop;
        public int Score { get; set; } = 0;
        public SnakeDirection NewDirection { get; set; } = SnakeDirection.Right;

        public event EventHandler GameLoopExecuted = delegate { };

        private const int GameLoopMs = 250;

        private Logger _logger;

        public SnakeService(Logger logger)
        {
            _logger = logger;
            _logger.Info("SnakeService CTOR");
            _gameLoop = new Timer(GameLoop, null, 0, 0);
            Init();
        }

        private string ObjectToString(object o)
        {
            return Convert.ToString(o).ToLowerInvariant();
        }

        private Random _random = new Random();
        private List<(int, int)> Blocks = new List<(int, int)>();
        private List<(int, int)> Player = new List<(int, int)>();
        private (int, int) Apple;

        private Timer _gameLoop;
        private int _gameLoopMs = GameLoopMs;

        private SnakeDirection _currentDirection = SnakeDirection.Right;

        //protected override async Task OnAfterRenderAsync()
        //{
        //    Logger.Info("OnAfterRenderAsync");
        //    // TEMPORARY: Currently we need this guard to avoid making the interop
        //    // call during prerendering. Soon this will be unnecessary because we
        //    // will change OnAfterRenderAsync so that it won't run during the
        //    // prerendering phase.
        //    if (!ComponentContext.IsConnected)
        //    {
        //        return;
        //    }
        //}

        public void StartGame()
        {
            Init();
        }

        private void Init()
        {
            Score = 0;
            CurrentState = SnakeGameState.Running;

            _gameLoopMs = GameLoopMs;
            _currentDirection = SnakeDirection.Right;
            NewDirection = SnakeDirection.Right;

            Blocks.Clear();
            for (int y = 0; y < MapSize; ++y)
            {
                for (int x = 0; x < MapSize; ++x)
                {
                    if (x == 0 || y == 0 || x == MapSize - 1 || y == MapSize - 1)
                    {
                        Blocks.Add((x, y));
                    }
                }
            }

            Player.Clear();
            for (int i = 0; i < 5; ++i)
            {
                Player.Add((4 + i, 4));
            }

            UpdateApple();
            StartGameLoop();
        }

        private void StartGameLoop()
        {
            _gameLoop.Change(_gameLoopMs, _gameLoopMs);
        }

        public string HandleInput(SnakeDirection direction)
        {
            NewDirection = direction;
            return "Thanks from C#!";
        }

        private void GameLoop(object _)
        {
            _logger.Info("Game Loop!");

            if (CurrentState != SnakeGameState.Running)
            {
                return;
            }

            UpdateScreen();

            GameLoopExecuted(this, EventArgs.Empty);
            //this.StateHasChanged();
        }

        private void UpdateApple()
        {
            int x = 0;
            int y = 0;
            do
            {
                x = _random.Next(0, MapSize);
                y = _random.Next(0, MapSize);
            } while (Blocks.Contains((x, y)) || Player.Contains((x, y)));

            Apple = (x, y);
        }

        private void UpdateDirection()
        {
            if ((_currentDirection == SnakeDirection.Up && NewDirection == SnakeDirection.Down) ||
                (_currentDirection == SnakeDirection.Down && NewDirection == SnakeDirection.Up) ||
                (_currentDirection == SnakeDirection.Right && NewDirection == SnakeDirection.Left) ||
                (_currentDirection == SnakeDirection.Left && NewDirection == SnakeDirection.Right))
            {
                return;
            }

            _currentDirection = NewDirection;
        }

        private void UpdateScreen()
        {
            UpdateDirection();

            // Move Player
            var (pX, pY) = Player.Last();
            switch (_currentDirection)
            {
                case SnakeDirection.Up:
                    pY = pY - 1;
                    break;
                case SnakeDirection.Down:
                    pY = pY + 1;
                    break;
                case SnakeDirection.Left:
                    pX = pX - 1;
                    break;
                case SnakeDirection.Right:
                    pX = pX + 1;
                    break;
            }

            // Game Logic
            if (Blocks.Contains((pX, pY)))
            {
                CurrentState = SnakeGameState.GameOver;
            }

            if (Player.Contains((pX, pY)))
            {
                CurrentState = SnakeGameState.GameOver;
            }

            //if (pX < 0) pX = MapSize - 1;
            //if (pX >= MapSize) pX = 0;
            //if (pY < 0) pY = MapSize - 1;
            //if (pY >= MapSize) pY = 0;
            Player.RemoveAt(0);
            Player.Add((pX, pY));

            if (Player.Contains(Apple))
            {
                Score++;
                if (Score < 20)
                {
                    _gameLoopMs = _gameLoopMs - 10;
                    _gameLoop.Change(_gameLoopMs, _gameLoopMs);
                }
                Player.Insert(0, (pX, pY));
                UpdateApple();
            }

            // Update Screen
            for (int y = 0; y < MapSize; ++y)
            {
                for (int x = 0; x < MapSize; ++x)
                {
                    if (Player.Contains((x, y)))
                    {
                        Screen[x, y] = SnakeTile.Player;
                    }
                    else if (Blocks.Contains((x, y)))
                    {
                        Screen[x, y] = SnakeTile.Block;
                    }
                    else if (Apple == (x, y))
                    {
                        Screen[x, y] = SnakeTile.Apple;
                    }
                    else
                    {
                        Screen[x, y] = SnakeTile.Empty;
                    }
                }
            }
        }
    }
}
