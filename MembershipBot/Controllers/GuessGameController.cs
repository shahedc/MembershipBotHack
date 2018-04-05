using MembershipBot.Models;
using System;

namespace MembershipBot.Controllers
{
    public static class GuessGameController
    {


        public static void StartGame(GuessGame game)
        {
            game.NumberToGuess = (new Random()).Next(game.MinGuess, game.MaxGuess);
            game.Tries = 0;
            game.InProgress = true;
        }

        public static GameTurnOutcome PlayTurn(GuessGame game, string valueGuessed)
        {

            game.Tries++;

            if (!int.TryParse(valueGuessed, out int numberGuessed))
            {
                return GameTurnOutcome.InvalidEntry;
            }

            if (numberGuessed == game.NumberToGuess)
            {
                return GameTurnOutcome.Guessed;
            }

            if (game.Tries == game.MaxTries)
            {
                return GameTurnOutcome.Lost;
            }

            if (numberGuessed < game.NumberToGuess)
            {
                return GameTurnOutcome.MissedLow;
            }

            return GameTurnOutcome.MissedHigh;
        }
    }
}
