using Newtonsoft.Json;
using System.IO;
using MembershipBot.Models;
using System.Collections.Generic;

namespace MembershipBot.Services
{
    public static class TeamDataService
    {

        private static DataStore store;

        static TeamDataService()
        {
            if (store is null)
            { 
                string fileName = "Services\\MembershipData.json";
                string jsonData = (new StreamReader(fileName)).ReadToEnd();

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    ObjectCreationHandling = ObjectCreationHandling.Reuse
                };

                store = JsonConvert.DeserializeObject<DataStore>(jsonData, settings);
            }
        }

        public static List<Member> GetMembersByName(string input)
        {
            return store.Members.FindAll((m) => string.Format("{0} {1}", m.FirstName, m.LastName).ToLower().Trim().IndexOf(input.ToLower().Trim()) >= 0) ?? new List<Member>(); 
        }

        public static List<Team> GetTeamsByName(string input)
        {
            return store.Teams.FindAll((t) => t.Name.IndexOf(input) >= 0) ?? new List<Team>();
        }

        public static Team GetTeam(int teamID)
        {
            return store.Teams.Find((t) => t.Id == teamID) ?? null;
        }

        public static Member GetMember(int memberId)
        {
            return store.Members.Find((t) => t.Id == memberId) ?? null;
        }

        public static List<Team> GetSharedTeams(int firstMemberId, int secondMemberId)
        {
            Member ma = GetMember(firstMemberId);
            Member mb = GetMember(secondMemberId);
            List<Team> sharedTeams = new List<Team>();

            if (!(ma is null) && !(mb is null))
            {
                List<TeamRole> sharedtr = ((List<TeamRole>)ma.TeamRoles).FindAll((tr) => (((List<TeamRole>)mb.TeamRoles).FindAll((trb) => trb.Team == tr.Team)?.Count > 0));
                
                foreach (TeamRole tr in sharedtr)
                {
                    if (!sharedTeams.Contains(tr.Team))
                    {
                        sharedTeams.Add(tr.Team);
                    }
                }
            }

            return sharedTeams;
        }

        public static Role GetRole(int roleId)
        {
            return store.Roles.Find((r) => r.Id == roleId) ?? null;
        }

        public static List<Member> GetManagers()
        {
            List<Member> managers = new List<Member>();
            store.Roles.FindAll((r) => r.IsManager)?.ForEach((r) => r.TeamMembers.ForEach((m) =>
            {
                if (!managers.Contains(m.Member))
                {
                    managers.Add(m.Member);
                }
            }));

            return managers;
        }

        public static List<TeamMember>  GetManagersForTeams()
        {
            List<TeamMember> teamManagers = new List<TeamMember>();

            store.Roles.FindAll((r) => r.IsManager)?.ForEach((r) => r.TeamMembers.ForEach((tm) => { teamManagers.Add(tm); }));

            return teamManagers;
        }

        public static List<Member> GetManagersForMember(int memberId)
        {
            Member m = GetMember(memberId);
            List<Member> managers = new List<Member>();

            if (!(m is null))
            {
                List<Team> teams = new List<Team>();

                ((List<TeamRole>)(m.TeamRoles)).ForEach((tr) => { teams.Add(tr.Team); });
                
                foreach (Team t in teams)
                {
                    foreach (MemberRole mr in t.MemberRoles)
                    {
                        if (mr.Role.IsManager && mr.Member.Id != memberId)
                        {
                            managers.Add(mr.Member);
                        }
                    }
                }
            }
            return managers;
        }

        public static List<Team> GetTeamsForMember(int memberId)
        {
            List<Team> teams = new List<Team>();

            (store.Members.Find((m) => m.Id == memberId)?.TeamRoles as List<TeamRole>)?.ForEach((tr) => teams.Add(tr.Team));

            return teams;
        }
    }
}