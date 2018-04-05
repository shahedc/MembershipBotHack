using System.Threading.Tasks;
using MembershipBot.Models;
using MembershipBot.Topics;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using PromptlyBot;

namespace MembershipBot
{
    public class MembershipBotConversationState : PromptlyBotConversationState<RootTopicState>
    {
       
    }

    public class MembershipBotUserState : StoreItem
    {
        public GuessGame GuessGame { get; set; }

    }

    public class MembershipBot : IBot
    {
        public Task OnReceiveActivity(ITurnContext context)
        {
            var rootTopic = new Topics.RootTopic(context);

            rootTopic.OnReceiveActivity(context);

            return Task.CompletedTask;
        }
    }

}