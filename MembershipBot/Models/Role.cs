using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipBot.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Member> Members { get; set; }
        public bool IsManager { get; set; }
    }
}
