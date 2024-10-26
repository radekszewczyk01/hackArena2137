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
    public class BotReceiver
    {
        public bool IsEntityInRowOrColumn(Tile[,] map)
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

            // Jeśli nie znaleziono naszego czołgu, kończymy metodę
            if (!ownTankPosition.HasValue)
                return false;

            int tankRow = ownTankPosition.Value.row;
            int tankCol = ownTankPosition.Value.col;

            // Sprawdzamy cały rząd
            for (int i = 0; i < colCount; i++)
            {
                if (i != tankCol && map[tankRow, i].Entities.Length > 0)
                {
                    return true; // Znaleziono jednostkę w naszym rzędzie
                }
            }

            // Sprawdzamy całą kolumnę
            for (int i = 0; i < rowCount; i++)
            {
                if (i != tankRow && map[i, tankCol].Entities.Length > 0)
                {
                    return true; // Znaleziono jednostkę w naszej kolumnie
                }
            }

            // Brak jednostek w naszym rzędzie ani kolumnie
            return false;
        }


    }
}
