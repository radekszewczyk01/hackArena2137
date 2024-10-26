using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTanksBotLogic;
using MonoTanksBotLogic.Enums;
using MonoTanksBotLogic.Models;

namespace BotReceiver
{
    public class BotReceiverLogic
    {
        public Direction? GetEnemysPositionInRowOrColumn(Tile[,] map)
        {

            int rowCount = map.GetLength(0); // liczba wierszy mapy
            int colCount = map.GetLength(1); // liczba kolumn mapy

            // Szukanie pozycji naszego czołgu
            (int row, int col)? ownTankPosition = null;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    // Sprawdzenie, czy na kafelku jest nasz czołg
                    if (map[row, col].Entities.Any(entity => entity is Tile.OwnTank))
                    {
                        ownTankPosition = (row, col);
                        break;
                    }
                }
                if (ownTankPosition.HasValue)
                    break;
            }

            int tankRow = ownTankPosition.Value.row;
            int tankCol = ownTankPosition.Value.col;

            // Sprawdzamy cały rząd na prawo
            for (int i = tankCol+1; i < colCount; i++)
            {
                if (map[tankRow, i].Entities.Any(entity => entity is Tile.Wall))
                {
                    break; 
                }
                if (map[tankRow, i].Entities.Any(entity => entity is Tile.EnemyTank))
                {
                    return Direction.Right;
                }
            }

            // Sprawdzamy cały rząd na prawo
            for (int i = tankCol-1; i >= 0; i--)
            {
                if (map[tankRow, i].Entities.Any(entity => entity is Tile.Wall))
                {
                    break;
                }
                if (map[tankRow, i].Entities.Any(entity => entity is Tile.EnemyTank))
                {
                    return Direction.Left;
                }
            }

            // Sprawdzamy całą kolumnę w dół
            for (int i = tankRow+1; i < rowCount; i++)
            {
                if (map[i, tankCol].Entities.Any(entity => entity is Tile.Wall))
                {
                    break;
                }
                if (map[i, tankCol].Entities.Any(entity => entity is Tile.EnemyTank))
                {
                    return Direction.Down;
                }
            }

            // Sprawdzamy całą kolumnę w górę
            for (int i = tankRow - 1; i >= 0; i--)
            {
                if (map[i, tankCol].Entities.Any(entity => entity is Tile.Wall))
                {
                    break;
                }
                if (map[i, tankCol].Entities.Any(entity => entity is Tile.EnemyTank))
                {
                    return Direction.Up;
                }
            }

            // Brak jednostek w naszym rzędzie ani kolumnie
            return null;
        }

        public Direction? GetTurretDirection(Tile[,] map)
        {
            int rowCount = map.GetLength(0); // liczba wierszy mapy
            int colCount = map.GetLength(1); // liczba kolumn mapy
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    foreach (var entity in map[y, x].Entities)
                    {
                        
                        if (entity is Tile.OwnTank ownTank)
                        {
                            return ownTank.Turret.Direction;
                        }
                        
                    }
                }
            }
            return null;
        }


    }
}
