using MembershipBot.Controllers;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using PromptlyBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LUIS;

namespace MembershipBot.Topics
{

    public class MembershipTopicState : ConversationTopicState
    {

    }

    public struct MembershipIntents
    {
        public const string GetTeamForMember = "GetTeamForMember";
        public const string None = "None";
        public const string IsManager = "IsManager";
        public const string GetTeam = "GetTeam";
        public const string GetSharedTeam = "GetSharedTeam";
        public const string GetMember = "GetMember";
        public const string GetManagers = "GetManagers";
    }

    public class MembershipTopic : ConversationTopic<MembershipTopicState, Membership>
    {
        public override Task OnReceiveActivity(ITurnContext context)
        {
            if ((context.Activity.Type == ActivityTypes.Message) && (context.Activity.AsMessageActivity().Text.Length > 0))
            {

                // If there is an active topic, let it handle this turn until it completes.
                if (HasActiveTopic)
                {
                    ActiveTopic.OnReceiveActivity(context);
                    return Task.CompletedTask;
                }

                var message = context.Activity.AsMessageActivity();
                var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);

                if (luisResult != null)
                {
                    (string key, double score) = luisResult.GetTopScoringIntent();

                    this.ClearActiveTopic();
                    switch (key)
                    {
                        case MembershipIntents.GetManagers:

                            context.SendActivity(MembershipIntents.GetManagers);

                            break;
                        case MembershipIntents.GetMember:
                            context.SendActivity(MembershipIntents.GetMember);
                            break;
                        case MembershipIntents.GetSharedTeam:
                            context.SendActivity(MembershipIntents.GetSharedTeam);
                            break;
                        case MembershipIntents.GetTeam:
                            context.SendActivity(MembershipIntents.GetTeam);
                            break;
                        case MembershipIntents.GetTeamForMember:
                            context.SendActivity(MembershipIntents.GetTeamForMember);
                            break;
                        case MembershipIntents.IsManager:
                            context.SendActivity(MembershipIntents.IsManager);
                            break;
                        default:
                            context.SendActivity(MembershipIntents.None);
                            break;
                    }
                    return Task.CompletedTask;
                }

                // If the user wants to change the topic of conversation...
                //if (message.Text.ToLowerInvariant() == "add alarm")
                //{
                //    // Set the active topic and let the active topic handle this turn.
                //    this.SetActiveTopic(ADD_ALARM_TOPIC)
                //            .OnReceiveActivity(context);
                //    return Task.CompletedTask;
                //}

            }

            return Task.CompletedTask;
        }

    }
}
