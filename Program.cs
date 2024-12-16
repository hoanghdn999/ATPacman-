using System;
using System.Collections.Generic;

public class PacmanGame
{
    private string[,] gameMap;
    private bool[,] isDot; // Tracks which cells are dots
    private int pacmanX, pacmanY;
    private List<Ghost> ghosts;
    private int dotsRemaining;
    private const int width = 15;
    private const int height = 10;
    private static readonly ConsoleColor[] GhostColors = { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Magenta };

    public PacmanGame(int numberOfGhosts)
    {
        gameMap = new string[width, height];
        isDot = new bool[width, height];
        InitializeGame(numberOfGhosts);
    }

    private void InitializeGame(int numberOfGhosts)
    {
        pacmanX = 1;
        pacmanY = 1;
        ghosts = new List<Ghost>();
        for (int i = 0; i < numberOfGhosts; i++)
        {
            int ghostX = 2 + i * 2;
            int ghostY = 3 + (i % 2) * 2;
            ghosts.Add(new Ghost(ghostX, ghostY, i + 1, GhostColors[i % GhostColors.Length])); // Assign ghost IDs and colors
        }
        PopulateMap();
    }

    private void PopulateMap()
    {
        dotsRemaining = 0;
        for (int x = 0; x < gameMap.GetLength(0); x++)
        {
            for (int y = 0; y < gameMap.GetLength(1); y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1 || (x % 4 == 0 && y % 3 == 0))
                {
                    gameMap[x, y] = "#"; // Place walls
                }
                else
                {
                    gameMap[x, y] = ".";
                    isDot[x, y] = true; // Mark as a dot
                    dotsRemaining++;
                }
            }
        }

        gameMap[pacmanX, pacmanY] = "P";
        isDot[pacmanX, pacmanY] = false;
        dotsRemaining--; // Pacman's initial position does not count as a dot

        foreach (var ghost in ghosts)
        {
            gameMap[ghost.X, ghost.Y] = $"G{ghost.Id}";
            isDot[ghost.X, ghost.Y] = false; // No dot under the ghost
        }
    }

    public void StartGame()
    {
        bool isRunning = true;
        while (isRunning)
        {
            DrawMap();
            DisplayCoordinatesAndPredictions();
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: MovePacman(0, -1); break;
                case ConsoleKey.DownArrow: MovePacman(0, 1); break;
                case ConsoleKey.LeftArrow: MovePacman(-1, 0); break;
                case ConsoleKey.RightArrow: MovePacman(1, 0); break;
                case ConsoleKey.Escape: isRunning = false; break;
            }
            UpdateGhosts();

            // Check for game-over conditions
            if (CheckCollision())
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Game Over! Pacman was caught by a ghost.");
                Console.ResetColor();
                isRunning = false;
            }
            else if (dotsRemaining == 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Congratulations! Pacman cleared all dots and wins!");
                Console.ResetColor();
                isRunning = false;
            }
        }
    }

    private void DrawMap()
    {
        Console.Clear();
        for (int y = 0; y < gameMap.GetLength(1); y++)
        {
            for (int x = 0; x < gameMap.GetLength(0); x++)
            {
                string cell = gameMap[x, y];
                if (cell == "P")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (cell.StartsWith("G"))
                {
                    int ghostId = int.Parse(cell.Substring(1));
                    Console.ForegroundColor = GhostColors[(ghostId - 1) % GhostColors.Length];
                }
                else if (cell == "#")
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (cell == ".")
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.Write(cell + " ");
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }

    private void DisplayCoordinatesAndPredictions()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Pacman ({pacmanX};{pacmanY})");
        foreach (var ghost in ghosts)
        {
            string prediction = ghost.GetPrediction(gameMap, pacmanX, pacmanY);
            Console.ForegroundColor = ghost.Color;
            Console.WriteLine($"G{ghost.Id} ({ghost.X};{ghost.Y}) -> Next Move: {prediction}");
        }
        Console.ResetColor();
    }

    private void MovePacman(int dx, int dy)
    {
        int newX = pacmanX + dx;
        int newY = pacmanY + dy;

        // Check boundaries and walls
        if (newX >= 0 && newX < width && newY >= 0 && newY < height && gameMap[newX, newY] != "#")
        {
            // Check if Pacman moves into a ghost
            foreach (var ghost in ghosts)
            {
                if (ghost.X == newX && ghost.Y == newY)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Game Over! Pacman was caught by a ghost.");
                    Console.ResetColor();
                    Environment.Exit(0); // Exit the game immediately
                }
            }

            // If no ghost is present, move Pacman
            if (isDot[newX, newY])
            {
                dotsRemaining--; // Pacman eats a dot
                isDot[newX, newY] = false;
            }

            gameMap[pacmanX, pacmanY] = isDot[pacmanX, pacmanY] ? "." : " "; // Restore dot or empty space
            pacmanX = newX;
            pacmanY = newY;
            gameMap[pacmanX, pacmanY] = "P";
        }
    }


    private void UpdateGhosts()
    {
        foreach (var ghost in ghosts)
        {
            ghost.Move(gameMap, pacmanX, pacmanY, isDot);

            // Check if a ghost collides with Pacman after moving
            if (ghost.X == pacmanX && ghost.Y == pacmanY)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Game Over! A ghost caught Pacman.");
                Console.ResetColor();
                Environment.Exit(0); // Exit the game immediately
            }
        }
    }


    private bool CheckCollision()
    {
        foreach (var ghost in ghosts)
        {
            if (ghost.X == pacmanX && ghost.Y == pacmanY)
                return true;
        }
        return false;
    }
}

public class Ghost
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Id { get; private set; }
    public ConsoleColor Color { get; private set; }
    private Random random = new Random();

    public Ghost(int x, int y, int id, ConsoleColor color)
    {
        X = x;
        Y = y;
        Id = id; // Assign unique ID to the ghost
        Color = color;
    }

    public void Move(string[,] map, int pacmanX, int pacmanY, bool[,] isDot)
    {
        int[] dx = { 1, -1, 0, 0 }; // Right, Left, Down, Up
        int[] dy = { 0, 0, 1, -1 };
        List<int> validMoves = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            int newX = X + dx[i];
            int newY = Y + dy[i];
            if (newX >= 0 && newX < map.GetLength(0) && newY >= 0 && newY < map.GetLength(1)
                && map[newX, newY] != "#" && !map[newX, newY].StartsWith("G"))
            {
                validMoves.Add(i);
            }
        }

        if (validMoves.Count > 0)
        {
            int move = validMoves[random.Next(validMoves.Count)];
            int newX = X + dx[move];
            int newY = Y + dy[move];
            map[X, Y] = isDot[X, Y] ? "." : " "; // Restore dot or empty space
            X = newX;
            Y = newY;
            map[X, Y] = $"G{Id}";
        }
    }

    public string GetPrediction(string[,] map, int pacmanX, int pacmanY)
    {
        int[] dx = { 1, -1, 0, 0 }; // Right, Left, Down, Up
        int[] dy = { 0, 0, 1, -1 };
        string[] directions = { "Right", "Left", "Down", "Up" };
        double[] probabilities = new double[4];
        double totalProbability = 0;

        for (int i = 0; i < 4; i++)
        {
            int newX = X + dx[i];
            int newY = Y + dy[i];
            if (newX >= 0 && newX < map.GetLength(0) && newY >= 0 && newY < map.GetLength(1)
                && map[newX, newY] != "#" && !map[newX, newY].StartsWith("G"))
            {
                probabilities[i] = 100.0 / (Math.Abs(pacmanX - newX) + Math.Abs(pacmanY - newY) + 1);
                totalProbability += probabilities[i];
            }
        }

        List<string> predictions = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            if (probabilities[i] > 0)
            {
                int percentage = (int)((probabilities[i] / totalProbability) * 100);
                predictions.Add($"{percentage}% {directions[i]}");
            }
        }

        return string.Join(", ", predictions);
    }
}

public class Program
{
    public static void Main()
    {
        PacmanGame game = new PacmanGame(4); 
        game.StartGame();
    }
}
