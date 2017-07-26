using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XamUBot
{
	public class LuisConstants
	{
		public const string IntentPrefix_Team = "[Team]";
		public const string Intent_GetSpecialist = "[Team] Get specialist";
		public const string Intent_ShowTeamMember = "[Team] Show team member";
		public const string Intent_ShowEntireTeam = "[Team] Show entire team";

		public const string Entity_Technology = "technology";
		public const string Entity_Trainer = "trainer";
	}
}