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
        public BotResponse? DecisionToAttack(Tile[,] map)
        {
            Direction? turretDirection = GetTurretDirection(map);
            Direction? enemyDirection = GetEnemysPositionInRowOrColumn(map);
            if (!turretDirection.HasValue || !enemyDirection.HasValue)
            {
                return null;
            }

            int difference = (int)enemyDirection - (int)turretDirection;

            int stepsRight = (difference + 4) % 4;   
            int stepsLeft = (4 - stepsRight) % 4;
            if (stepsRight == 0)
            {
                return BotResponse.UseAbility(AbilityType.FireBullet); // Robot jest już obrócony w stronę przeciwnika
            }
            else if (stepsRight <= stepsLeft)
            {
                return BotResponse.Rotate(null, Rotation.Right); // Obrót w prawo jest krótszy
            }
            else
            {
                return BotResponse.Rotate(null, Rotation.Left);  // Obrót w lewo jest krótszy
            }
        }

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

        public Stack<BotResponse> GeneratePathToNearestZone(Tile[,] map, int? targetZoneIndex)
        {
            if (targetZoneIndex == null)
            {
                var instructions = new Stack<BotResponse>();
                instructions.Push(BotResponse.Pass());
                return instructions;
            }
            // Szukanie pozycji naszego czołgu
            var ownTankPosition = FindOwnTankPosition(map);
            if (ownTankPosition == null) return null; // Jeśli nie znaleziono czołgu, zwróć null

            // Wyciągnij kierunek początkowy naszego czołgu
            var ownTank = map[ownTankPosition.Value.row, ownTankPosition.Value.col]
                .Entities.OfType<Tile.OwnTank>().FirstOrDefault();

            if (ownTank == null) return null; // Jeśli nie znaleziono czołgu, zwróć null

            // Znajdź ścieżkę do najbliższego kafelka w strefie
            var path = BFS(map, ownTankPosition.Value, (int)targetZoneIndex);
            if (path == null) return null; // Jeśli brak ścieżki, zwróć null

            // Przetwórz ścieżkę na stos instrukcji
            return GenerateInstructions(path, ownTank.Direction);
        }

        // Implementacja BFS do znajdowania najkrótszej ścieżki
        private List<(int x, int y)> BFS(Tile[,] map, (int row, int col) start, int targetZoneIndex)
        {
            var directions = new (int row, int col)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            var queue = new Queue<((int row, int col) pos, List<(int row, int col)> path)>();
            var visited = new HashSet<(int row, int col)>();

            queue.Enqueue((start, new List<(int row, int col)> { start }));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (current, path) = queue.Dequeue();
                var (cr, cc) = current;

                if (map[cr, cc].ZoneIndex == targetZoneIndex)
                    return path;

                foreach (var (dr, dc) in directions)
                {
                    var nr = cr + dr;
                    var nc = cc + dc;
                    if (nr <= 0) continue;
                    if (nc <= 0) continue;
                    if (nr >= map.GetLength(0)) continue;
                    if(nc >= map.GetLength(1)) continue;

                    if (nr >= 0 && nr < map.GetLength(0) && nc >= 0 && nc < map.GetLength(1) &&
                        !visited.Contains((nr, nc)) && !IsWall(map[nr, nc]))
                    {
                        visited.Add((nr, nc));
                        var newPath = new List<(int row, int col)>(path) { (nr, nc) };
                        queue.Enqueue(((nr, nc), newPath));
                    }
                }
            }
            return null;
        }

        // Sprawdza, czy kafelek jest ścianą
        private bool IsWall(Tile tile) => tile.Entities.Any(e => e is Tile.Wall);

        // Funkcja znajdująca pozycję własnego czołgu na mapie
        private (int row, int col)? FindOwnTankPosition(Tile[,] map)
        {
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    if (map[row, col].Entities.Any(entity => entity is Tile.OwnTank))
                        return (row, col);
                }
            }
            return null;
        }

        // Generowanie instrukcji na podstawie ścieżki
        private Stack<BotResponse> GenerateInstructions(List<(int row, int col)> path, Direction startDirection)
        {
            var instructions = new Stack<BotResponse>();
            Direction currentDirection = startDirection;

            for (int i = 1; i < path.Count; i++)
            {
                var (cr, cc) = path[i - 1];
                var (nr, nc) = path[i];
                var desiredDirection = GetDirection((nr - cr, nc - cc));

                if (currentDirection != desiredDirection)
                {
                    int rotationCount = (desiredDirection - currentDirection + 4) % 4;

                    switch (rotationCount)
                    {
                        case 1:
                            instructions.Push(BotResponse.Rotate(Rotation.Right, null));
                            break;
                        case 2:
                            instructions.Push(BotResponse.Rotate(Rotation.Right, null));
                            instructions.Push(BotResponse.Rotate(Rotation.Right, null));
                            break;
                        case 3:
                            instructions.Push(BotResponse.Rotate(Rotation.Left, null));
                            break;
                    }

                    currentDirection = desiredDirection;
                }
                instructions.Push(BotResponse.Move(MovementDirection.Forward));
            }
            var ans = new Stack<BotResponse>();
            while (instructions.Count()!=0)
            {
                ans.Push(instructions.Pop());
            }


            return ans;
        }

        // Wyznacza kierunek na podstawie różnicy współrzędnych
        private Direction GetDirection((int rowDelta, int colDelta) delta) =>
            delta switch
            {
                (0, 1) => Direction.Right,
                (0, -1) => Direction.Left,
                (1, 0) => Direction.Down,
                (-1, 0) => Direction.Up,
                _ => throw new InvalidOperationException("Nieprawidłowy kierunek ruchu")
            };

        // Generowanie instrukcji obrotu na podstawie bieżącego i docelowego kierunku
        private BotResponse GetRotationInstruction(Direction current, Direction desired)
        {
            int rotationCount = (desired - current + 4) % 4;

            return rotationCount switch
            {
                1 => BotResponse.Rotate(Rotation.Right, null), // 90 stopni w prawo
                2 => BotResponse.Rotate(Rotation.Right, null), // 180 stopni
                3 => BotResponse.Rotate(Rotation.Left, null), // 90 stopni w lewo
                _ => BotResponse.Pass()
            };
        }

        public bool IsTankInAnyZone(Tile[,] map)
        {
            int rowCount = map.GetLength(0);
            int colCount = map.GetLength(1);

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    // Sprawdzenie, czy na kafelku jest nasz czołg
                    if (map[row, col].Entities.Any(entity => entity is Tile.OwnTank))
                    {
                        // Sprawdzenie, czy kafelek posiada ZoneIndex
                        return map[row, col].ZoneIndex.HasValue;
                    }
                }
            }

            // Jeśli nie znaleziono czołgu, zwraca false
            return false;
        }



        public int? FindAnyZoneIndex(Tile[,] map)
        {
            int rowCount = map.GetLength(0);
            int colCount = map.GetLength(1);

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    // Sprawdzenie, czy kafelek ma ustawiony ZoneIndex
                    if (map[row, col].ZoneIndex.HasValue)
                    {
                        return map[row, col].ZoneIndex; // Zwróć pierwszy napotkany ZoneIndex
                    }
                }
            }

            // Jeśli nie znaleziono żadnego ZoneIndex, zwróć null
            return null;
        }

        public void PrintOwnTankPosition(Tile[,] map)
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

            // Sprawdzenie, czy znaleziono pozycję czołgu
            if (ownTankPosition.HasValue)
            {
                int tankRow = ownTankPosition.Value.row;
                int tankCol = ownTankPosition.Value.col;

                // Wypisanie pozycji czołgu
                Console.WriteLine($"Pozycja czołgu: wiersz = {tankRow}, kolumna = {tankCol}");
            }
            else
            {
                Console.WriteLine("Czołg nie został znaleziony na mapie.");
            }
        }

        public void WypiszListe(List<(int x, int y)> lista)
        {
            foreach (var (x, y) in lista)
            {
                Console.WriteLine($"x: {x}, y: {y}");
            }
        }



    }



}

