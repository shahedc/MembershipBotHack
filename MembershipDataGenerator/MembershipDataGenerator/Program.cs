using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MembershipDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            DataStore store = new DataStore();
            store.Members = new List<Member>();
            store.Roles = new List<Role>();
            store.Teams = new List<Team>();

            Role role1 = new Role() { Id = 1, IsManager = true, Name = "Manager" };
            role1.TeamMembers = new List<TeamMember>();
            Role role2 = new Role() { Id = 2, IsManager = false, Name = "Engineer" };
            role2.TeamMembers = new List<TeamMember>();
            Role role3 = new Role() { Id = 3, IsManager = false, Name = "Analyst" };
            role3.TeamMembers = new List<TeamMember>();
            store.Roles.Add(role1);
            store.Roles.Add(role2);
            store.Roles.Add(role3);

            Member member1 = new Member() { Id = 1, FirstName = "John", LastName = "Doe" };
            member1.TeamRoles = new List<TeamRole>();
            Member member2 = new Member() { Id = 2, FirstName = "Jane", LastName = "Smith" };
            member2.TeamRoles = new List<TeamRole>();
            Member member3 = new Member() { Id = 3, FirstName = "Jill", LastName = "Jones" };
            member3.TeamRoles = new List<TeamRole>();
            store.Members.Add(member1);
            store.Members.Add(member2);
            store.Members.Add(member3);

            Team team1 = new Team() { Id = 1, Name = "Team A", Description = "Team A Description" };
            team1.MemberRoles = new List<MemberRole>();
            Team team2 = new Team() { Id = 2, Name = "Team B", Description = "Team B Description" };
            team2.MemberRoles = new List<MemberRole>();
            store.Teams.Add(team1);
            store.Teams.Add(team2);

            TeamMember teamMember1 = new TeamMember() { Team = team1, Member = member1 };
            role1.TeamMembers.Add(teamMember1);
            TeamMember teamMember2 = new TeamMember() { Team = team1, Member = member2 };
            role2.TeamMembers.Add(teamMember2);
            TeamMember teamMember3 = new TeamMember() { Team = team2, Member = member3 };
            role1.TeamMembers.Add(teamMember3);
            TeamMember teamMember4 = new TeamMember() { Team = team2, Member = member1 };
            role2.TeamMembers.Add(teamMember4);

            MemberRole memberRole1 = new MemberRole() { Member = member1, Role = role1 };
            team1.MemberRoles.Add(memberRole1);
            MemberRole memberRole2 = new MemberRole() { Member = member2, Role = role2 };
            team1.MemberRoles.Add(memberRole2);
            MemberRole memberRole3 = new MemberRole() { Member = member3, Role = role1 };
            team2.MemberRoles.Add(memberRole3);
            MemberRole memberRole4 = new MemberRole() { Member = member1, Role = role2 };
            team2.MemberRoles.Add(memberRole4);

            TeamRole teamRole1 = new TeamRole() { Team = team1, Role = role1 };
            member1.TeamRoles.Add(teamRole1);
            TeamRole teamRole2 = new TeamRole() { Team = team1, Role = role2 };
            member2.TeamRoles.Add(teamRole2);
            TeamRole teamRole3 = new TeamRole() { Team = team2, Role = role1 };
            member3.TeamRoles.Add(teamRole3);
            TeamRole teamRole4 = new TeamRole() { Team = team2, Role = role2 };
            member1.TeamRoles.Add(teamRole4);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;

            string json = JsonConvert.SerializeObject(store, settings);
            Console.WriteLine(json);

            DataStore ds = JsonConvert.DeserializeObject<DataStore>(json);

            Console.ReadLine();


        }
    }
}
