using System.Threading.Tasks;
using MembershipBot.Controllers;
using MembershipBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using PromptlyBot;
using PromptlyBot.Prompts;
using PromptlyBot.Validator;

namespace MembershipBot.Topics
{
    internal struct NumberPromptFailureReasons
    {
        public const string NotNumeric = "NotNumeric";
        public const string TooManyAttempts = "toomanyattempts";
    }

    public class GuessGameTopicState : ConversationTopicState
    {
        public GuessGame game = new GuessGame();
    }

    public class GameTopic : ConversationTopic<GuessGameTopicState, GuessGame>
    {
        public GameTopic() : base()
        {
            this.SubTopics.Add("MagicNumberPrompt", (object[] args) =>
            {
                Prompt<int> numberPrompt = new Prompt<int>();

                numberPrompt.Set
                .OnPrompt((context, lastTurnReason) =>
                    {
                        if ((lastTurnReason != null) && (lastTurnReason == NumberPromptFailureReasons.NotNumeric))
                        {
                            context.SendActivity("Sorry, only numeric values are accepted.", "Let's try again.");
                        }
                    })
                .Validator(new MagicNumberValidator())
                .MaxTurns(3)
                .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();
                        this.OnReceiveActivity(context);
                    })
                .OnFailure((context, reason) =>
                {
                    this.ClearActiveTopic();
                    if ((reason != null) && (reason == NumberPromptFailureReasons.TooManyAttempts))
                    {
                        context.SendActivity("I'm sorry I'm having issues understanding you.");
                    }

                    this.OnFailure(context, reason);

                });
                return numberPrompt;
            });
        }

        public override Task OnReceiveActivity(ITurnContext context)
        {
            if (HasActiveTopic)
            {
                ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }
            GuessGame thisGame = this.State.game;
            if (!this.State.game.InProgress)
            {

                GuessGameController.StartGame(thisGame);
                context.SendActivity($"Hey! Let's play!\nGuess the number I'm thinking of." +
                    $" It's an integer between {thisGame.MinGuess} and {thisGame.MaxGuess}. You can guess {thisGame.MaxTries} times.");
                return Task.CompletedTask;
            }
            else
            {

                string message = "";
                GameTurnOutcome outcome = GuessGameController.PlayTurn(thisGame, context.Activity.Text);

                switch (outcome)
                {
                    case GameTurnOutcome.Guessed:
                        message = "Congrats! You guessed the number!";
                        break;
                    case GameTurnOutcome.Lost:
                        message = $"Sorry... The number was {thisGame.NumberToGuess}.";
                        break;
                    case GameTurnOutcome.InvalidEntry:
                        message = $"That's not a valid number. The number I'm thinking of is between {thisGame.MinGuess} and {thisGame.MaxGuess}. Please try again.";
                        break;
                    case GameTurnOutcome.MissedHigh:
                        message = "That's not it - try a lower number.";
                        break;
                    default:
                        message = "That's not it - try a higher number.";
                        break;
                }

                if (outcome == GameTurnOutcome.Guessed || outcome == GameTurnOutcome.Lost)
                {
                    this.OnSuccess(context, thisGame);
                }
                else
                {
                    context.SendActivity(message);
                    this.SetActiveTopic("MagicNumberPrompt")
                        .OnReceiveActivity(context);
                        return Task.CompletedTask;
                }

                context.SendActivity(message);
                return Task.CompletedTask;
            }
        }
    }

    public class MagicNumberValidator : Validator<int>
    {
        public override ValidatorResult<int> Validate(ITurnContext context)
        {

            if (!int.TryParse(context.Activity.AsMessageActivity().Text.Trim(), out int number))
            {
                return new ValidatorResult<int>
                {
                    Reason = NumberPromptFailureReasons.NotNumeric
                };
            }

            else
            {
                return new ValidatorResult<int>
                {
                    Value = number
                };

            }
        }

    }
}


