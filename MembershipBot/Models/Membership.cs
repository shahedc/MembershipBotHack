using MembershipBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipBot.Controllers
{
    public class Membership
    {
        public Member Member { get; set; }
        public Team Team { get; set; }
        public Role Role { get; set; }
    }
}
