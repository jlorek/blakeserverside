using Bogus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlakeServerSide.Data
{
    public class SnakePlayer
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; set; }

        public bool IsAlive { get; set; }

        public int Score => Body.Count - InitialSize;

        public (int, int) Head => Body.Last();

        public List<(int, int)> Body { get; private set; } = new List<(int, int)>();

        public SnakeDirection CurrentDirection { get; set; } = SnakeDirection.Right;

        public SnakeDirection NewDirection { get; set; } = SnakeDirection.Right;

        public SnakeTile PlayerTile { get; set; }

        private const int InitialSize = 5;

        private const int WallOffset = 2;

        public SnakePlayer(SnakeTile playerTile)
        {
            Body = new List<(int, int)> { (0, 0) };
            PlayerTile = playerTile;

            var faker = new Faker();
            Name = faker.Name.FirstName();
        }

        internal void Init()
        {
            CurrentDirection = SnakeDirection.Right;
            NewDirection = SnakeDirection.Right;
            IsAlive = true;

            Body.Clear();

            if (PlayerTile == SnakeTile.PlayerOne)
            {
                for (int i = 0; i < InitialSize; ++i)
                {
                    Body.Add((WallOffset + i, WallOffset));
                }
                CurrentDirection = NewDirection = SnakeDirection.Right;
            }
            else if (PlayerTile == SnakeTile.PlayerTwo)
            {
                for (int i = 0; i < InitialSize; ++i)
                {
                    Body.Add((SnakeService.MapSize - WallOffset - i - 1, SnakeService.MapSize - WallOffset - 1));
                }
                CurrentDirection = NewDirection = SnakeDirection.Left;
            }
            else if (PlayerTile == SnakeTile.PlayerThree)
            {
                for (int i = 0; i < InitialSize; ++i)
                {
                    Body.Add((SnakeService.MapSize - WallOffset - i - 1, WallOffset));
                }
                CurrentDirection = NewDirection = SnakeDirection.Left;
            }
            else if (PlayerTile == SnakeTile.PlayerFour)
            {
                for (int i = 0; i < InitialSize; ++i)
                {
                    Body.Add((WallOffset + i, SnakeService.MapSize - WallOffset - 1));
                }
                CurrentDirection = NewDirection = SnakeDirection.Right;
            }
        }

        internal void UpdateDirection()
        {
            if ((CurrentDirection == SnakeDirection.Up && NewDirection == SnakeDirection.Down) ||
                (CurrentDirection == SnakeDirection.Down && NewDirection == SnakeDirection.Up) ||
                (CurrentDirection == SnakeDirection.Right && NewDirection == SnakeDirection.Left) ||
                (CurrentDirection == SnakeDirection.Left && NewDirection == SnakeDirection.Right))
            {
                return;
            }

            CurrentDirection = NewDirection;
        }

        internal (int, int) PeekNewPosition()
        {
            var (pX, pY) = Head;

            switch (CurrentDirection)
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

            // wrap-around-logic
            //if (pX < 0) pX = MapSize - 1;
            //if (pX >= MapSize) pX = 0;
            //if (pY < 0) pY = MapSize - 1;
            //if (pY >= MapSize) pY = 0;

            return (pX, pY);
        }

        internal void UpdatePosition()
        {
            var newHead = PeekNewPosition();
            Body.RemoveAt(0);
            Body.Add(newHead);
        }

        internal void Grow()
        {
            Body.Insert(0, Body.First());
        }
    }
}
