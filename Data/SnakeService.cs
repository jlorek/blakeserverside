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
        public List<SnakePlayer> Snakes { get; set; } = new List<SnakePlayer>();
        public SnakeGameState CurrentState { get; set; } = SnakeGameState.Stop;
        public event EventHandler GameLoopExecuted = delegate { };

        private const int DefaultGameLoopMs = 250;

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
        private List<(int, int)> Level = new List<(int, int)>();
        private (int, int) Apple;

        private Timer _gameLoop;
        private int _gameLoopMs = DefaultGameLoopMs;

        public Guid? RegisterNewPlayer()
        {
            if (Snakes.Count == 4)
            {
                return null;
            }

            var player = new SnakePlayer(GetNextPlayerTile());
            Snakes.Add(player);
            return player.Id;
        }

        private SnakeTile GetNextPlayerTile()
        {
            if (Snakes.Count == 0)
            {
                return SnakeTile.PlayerOne;
            }
            else if (Snakes.Count == 1)
            {
                return SnakeTile.PlayerTwo;
            }
            else if (Snakes.Count == 2)
            {
                return SnakeTile.PlayerThree;
            }
            else
            {
                return SnakeTile.PlayerFour;
            }
        }

        public void UnregisterPlayer(Guid id)
        {
            Snakes.RemoveAll(p => p.Id == id);
        }

        public void StartGame()
        {
            Init();
            CurrentState = SnakeGameState.Running;
            _gameLoop.Change(_gameLoopMs, _gameLoopMs);
        }

        private void Init()
        {
            _gameLoopMs = DefaultGameLoopMs;

            Level.Clear();
            for (int y = 0; y < MapSize; ++y)
            {
                for (int x = 0; x < MapSize; ++x)
                {
                    if (x == 0 || y == 0 || x == MapSize - 1 || y == MapSize - 1)
                    {
                        Level.Add((x, y));
                    }
                }
            }

            foreach (var snake in Snakes)
            {
                snake.Init();
            }

            PlaceApple();
        }

        public void HandleInput(Guid playerId, SnakeDirection direction)
        {
            var player = Snakes.Single(p => p.Id == playerId);
            player.NewDirection = direction;
        }

        private void GameLoop(object _)
        {
            if (CurrentState != SnakeGameState.Running)
            {
                return;
            }

            _logger.Info("Game Loop running!");

            UpdateSnakes();
            UpdateApple();
            UpdateScreen();

            GameLoopExecuted(this, EventArgs.Empty);
        }

        private void UpdateApple()
        {
            foreach (var snake in Snakes)
            {
                if (snake.Body.Contains(Apple))
                {
                    snake.Grow();

                    if (Snakes.Sum(s => s.Score) < 20)
                    {
                        _gameLoopMs = _gameLoopMs - 10;
                        _gameLoop.Change(_gameLoopMs, _gameLoopMs);
                    }

                    PlaceApple();
                }
            }
        }

        private void PlaceApple()
        {
            var newApplePosition = CalculatePossibleApplePosition();
            if (!newApplePosition.HasValue)
            {
                CurrentState = SnakeGameState.GameOver;
                return;
            }

            Apple = newApplePosition.Value;
        }

        private (int, int)? CalculatePossibleApplePosition()
        {
            List<(int, int)> possiblePositions = new List<(int, int)>();
            for (int y = 0; y < MapSize; ++y)
            {
                for (int x = 0; x < MapSize; ++x)
                {
                    possiblePositions.Add((x, y));
                }
            }

            possiblePositions.RemoveAll(p => Level.Contains(p));
            possiblePositions.RemoveAll(p => Snakes.SelectMany(s => s.Body).Contains(p));

            if (possiblePositions.Count == 0)
            {
                return null;
            }

            int possibleIndex = _random.Next(possiblePositions.Count);
            return possiblePositions[possibleIndex];
        }

        private void UpdateSnakes()
        {
            foreach (var snake in Snakes)
            {
                snake.UpdateDirection();
                var newHead = snake.PeekNewPosition();

                if (Level.Contains(newHead))
                {
                    snake.IsAlive = false;
                    CurrentState = SnakeGameState.GameOver;
                }

                if (snake.Body.Contains(newHead))
                {
                    snake.IsAlive = false;
                    CurrentState = SnakeGameState.GameOver;
                }

                snake.UpdatePosition();
            }
        }

        private void UpdateScreen()
        {
            // Update Screen
            for (int y = 0; y < MapSize; ++y)
            {
                for (int x = 0; x < MapSize; ++x)
                {
                    Screen[x, y] = SnakeTile.Empty;

                    foreach (var snake in Snakes)
                    {
                        if (snake.Body.Contains((x, y)))
                        {
                            Screen[x, y] = snake.PlayerTile;
                        }
                    }

                    if ((int) Screen[x, y] >= (int) SnakeTile.PlayerOne)
                    {
                        continue;
                    }
                    else if (Level.Contains((x, y)))
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
