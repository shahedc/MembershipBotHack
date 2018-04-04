using System.Threading.Tasks;
using MembershipBot.Models;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.LUIS;
using MembershipBot.Services;
using System.Collections.Generic;

namespace MembershipBot
{
    public class MembershipBot : IBot
    {
        public async Task OnReceiveActivity(ITurnContext context)
        {
            List<Team> teams = TeamDataService.GetSharedTeams(1, 3) ;

            var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
            ConversationCounter counter = context.GetConversationState<ConversationCounter>();
            if (context.Activity.Type is ActivityTypes.Message)
            {
                //counter.Counter++;
                string message = $"({counter.Counter++}) - Intent detected: {luisResult.GetTopScoringIntent().key} ({luisResult.GetTopScoringIntent().score})";

                foreach (var e in luisResult.Entities)
                {
                    message += $" \n \n -Entity: {e.Key} - Value: {e.Value}";
                }

                foreach (Team team in teams)
                {
                    message += $" \n \n Team Found: {team.Description} ({team.Id})";
                }

                await context.SendActivity(message);

            }

        }
    }
}