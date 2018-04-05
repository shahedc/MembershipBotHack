namespace MembershipBot.Models
{
    public enum GameTurnOutcome
    {
        MissedLow,
        MissedHigh,
        Guessed,
        Lost,
        InvalidEntry
    }

    public class GuessGame
    {

        public int MaxTries { get; set; } = 3;
        public int MinGuess { get; set; } = 0;
        public int MaxGuess { get; set; } = 10;
        public bool InProgress { get; set; }
        public int Tries { get; set; }
        public int NumberToGuess { get; set; }
    }
}
