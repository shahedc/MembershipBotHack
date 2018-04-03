using Newtonsoft.Json;
using System.IO;

namespace MembershipBot.Services
{
    public static class TeamDataService
    {

        private static DataStore store;

        static TeamDataService()
        {
            if (store is null)
            {
                string fileName = "Models\\MembershipData.json";
                string jsonData = (new StreamReader(fileName)).ReadToEnd();

                store = JsonConvert.DeserializeObject<DataStore>(jsonData);
            }
        }

        public static string GetMembersByName(string input)
        {
            return string.Empty;
        }

        public static string GetTeamsByName(string input)
        {
            return string.Empty;
        }

        public static string GetTeam(int teamID)
        {
            return string.Empty;
        }

        public static string GetMember(int memberId)
        {
            return string.Empty;
        }

        public static string GetSharedTeams(int memberIda, int memberIdb)
        {
            return string.Empty;
        }

        public static string GetRole(int roleId)
        {
            return string.Empty;
        }

        public static string GetManagers()
        {
            return string.Empty;
        }

        public static string GetManagersForTeams()
        {
            return string.Empty;
        }

        public static string GetManagersForMenber(int memberId)
        {
            return string.Empty;
        }

        public static string GetTeamsForMenber(int memberId)
        {
            return string.Empty;
        }
    }
}
