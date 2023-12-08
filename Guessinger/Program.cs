using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection.PortableExecutable;

namespace Guessinger
{
    class Program
    {
        // Lista med spelares poäng
        static List<Player> playerScores = new List<Player>();
        // Kopplingen till databasen
        static string connectionString = "Data Source=GuessingerPlayers.db;Version=3;";

        static void InitializeDatabase()
        {
            // Skapar databasen om den inte finns
            if (!File.Exists("GuessingerPlayers.db"))
            {
                SQLiteConnection.CreateFile("GuessingerPlayers.db");
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // Öppnar koppling mot databasen
                connection.Open();

                // Skapar tabellen i databasen för spelarna
                using (SQLiteCommand command = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS Players (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "Playername TEXT NOT NULL," +
                    "Highscore INTEGER NOT NULL)",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static void Main()
        {
            // Skapa / anslut till databasen
            InitializeDatabase();

            bool programRuns = true;

            while (programRuns)
            {
                Console.Clear();
                Console.WriteLine("Welcome to Guessinger!");
                Console.WriteLine("1. Play Guessinger");
                Console.WriteLine("2. View High Scores");
                Console.WriteLine("3. Exit");

                string choice = Console.ReadLine();
                Console.WriteLine($"Choice entered: '{choice}'");
                switch (choice)
                {
                    case "1":
                        PlayGuessinger();
                        break;
                    case "2":
                        ViewHighScores();
                        break;
                    case "3":
                        programRuns = false;
                        break;
                    default:
                        break;
                }
            }
        }

        static void LoadScores()
        {
            // Clear the existing scores before loading new scores
            playerScores.Clear();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Players ORDER BY Highscore DESC";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Player score = new Player
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Playername = Convert.ToString(reader["Playername"]),
                                Highscore = Convert.ToInt32(reader["Highscore"]),
                            };

                            playerScores.Add(score);
                        }
                    }
                }
                connection.Close();
            }
        }


        static void UpdateHighScore(Player player)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Check if a record with the same player name already exists
                string checkIfExistsQuery = "SELECT Id, Highscore FROM Players WHERE Playername = @Playername";
                using (SQLiteCommand checkIfExistsCommand = new SQLiteCommand(checkIfExistsQuery, connection))
                {
                    checkIfExistsCommand.Parameters.AddWithValue("@Playername", player.Playername);

                    using (SQLiteDataReader reader = checkIfExistsCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int existingHighscore = Convert.ToInt32(reader["Highscore"]);

                            // Check if the new high score is greater than the existing one
                            if (player.Highscore > existingHighscore)
                            {
                                // Player already exists, update the high score
                                string updateQuery = "UPDATE Players SET Highscore = @Highscore WHERE Id = @Id";
                                using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@Id", Convert.ToInt32(reader["Id"]));
                                    updateCommand.Parameters.AddWithValue("@Highscore", player.Highscore);

                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            // Player does not exist, insert a new record
                            string insertQuery = "INSERT INTO Players (Playername, Highscore) VALUES (@Playername, @Highscore)";
                            using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Playername", player.Playername);
                                insertCommand.Parameters.AddWithValue("@Highscore", player.Highscore);
                                
                                insertCommand.ExecuteNonQuery(); 

                            }
                        }
                    }
                }
                connection.Close();
            }
        }
        static void ViewHighScores()
        {
            Console.Clear();
            // Ladda spelarnas poäng
            LoadScores();
            Console.WriteLine("High Scores:");

            if (playerScores.Count == 0)
            {
                Console.WriteLine("\nNo high scores available.");
            }
            else
            {
                foreach (var player in playerScores)
                {
                    Console.WriteLine($"{player.Playername} - High Score: {player.Highscore}");
                }
            }

            Console.WriteLine("\nPress any key to return.");
            Console.ReadKey(); // Wait for user input before returning
        }


        static void PlayGuessinger()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Guessinger! Guess the number between 1 and 10.");

            string playerName = "";
            bool exitGame = false;

            do
            {
                Console.Write("Enter your name (or press Enter to exit): ");
                playerName = Console.ReadLine();

                if (string.IsNullOrEmpty(playerName))
                {
                    // Exit the game if Enter is pressed without entering a name
                    exitGame = true;
                    break;
                }

                if (string.IsNullOrWhiteSpace(playerName))
                {
                    Console.Write("Name can't be left empty.\n");
                    continue;
                }

                // Generate a random number for the player to guess
                Random random = new Random();
                int correctNumber = random.Next(1, 11);

                int points = 0;
                bool guessedCorrectly = true;

                while (guessedCorrectly)
                {
                    Console.Write("Enter your guess (or enter 0 to exit): ");
                    string input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        Console.Write("Guess can't be left empty.\n");
                        continue;
                    }

                    if (int.TryParse(input, out int playerGuess))
                    {
                        // Successfully parsed as an integer
                        if (playerGuess == 0)
                        {
                            // Exit the loop if the player chooses to stop guessing
                            break;
                        }

                        if (playerGuess == correctNumber)
                        {
                            Console.WriteLine("Congratulations! You guessed the correct number.");
                            points++;
                            Console.WriteLine($"You earned {points} point{(points != 1 ? "s" : "")}.");

                            // Generate a new random number for the next guess
                            correctNumber = random.Next(1, 11);
                        }
                        else
                        {
                            guessedCorrectly = false;
                            Console.WriteLine($"Sorry, the correct number was {correctNumber}. Try again!");
                            break;
                        }
                    }
                    else
                    {
                        Console.Write("Invalid input. Please enter a valid number.\n");
                    }
                }

                Console.WriteLine($"Your high score is: {points} points.");
                Player currentPlayer = new Player { Playername = playerName, Highscore = points };
                UpdateHighScore(currentPlayer);

                Console.WriteLine("\nPress Enter to return.");
            } while (!exitGame && Console.ReadKey().Key != ConsoleKey.Escape);

            LoadScores();
        }
    }
}
