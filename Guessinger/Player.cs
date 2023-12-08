namespace Guessinger {
    /// <summary>
    /// This representes the players character.
    /// </summary>
    class Player
    {
        public int Id { get; set; } // SQLite requires an auto-incrementing integer as the primary key
        public string Playername { get; set; }
        public int Highscore { get; set; }
    }
}