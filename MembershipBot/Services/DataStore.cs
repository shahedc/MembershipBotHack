using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MembershipBot.Models;

namespace MembershipBot.Services
{
    public class DataStore
    {
        public List<Member> Members { get; set; }
        public List<Team> Teams { get; set; }
        public List<Role> Roles { get; set; }
    }
}
