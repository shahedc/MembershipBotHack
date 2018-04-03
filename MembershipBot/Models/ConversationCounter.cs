using Microsoft.Bot.Builder.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipBot.Models
{
    public class ConversationCounter : StoreItem
    {
        public int Counter { get; set; }
    
    }
}
