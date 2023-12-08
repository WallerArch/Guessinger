using System;

namespace Guessinger {
    /// <summary>
    /// This representes the players character.
    /// </summary>
   internal class Player
    {
        private int Id { get; set; } // SQLite requires an auto-incrementing integer as the primary key
        private string Name { get; set; }
        private string Highscore { get; set; }
    }
}