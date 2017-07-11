using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;
using System.Collections.Generic;
using System.Linq;

namespace XamUBot.Dialogs
{
	[Serializable]
	public class TeamDialog : IDialog<object>
	{
		IList<TeamResponse> _teamList;

		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync("What would you like to know about our teams?");
			_teamList = await ApiManager.Instance.GetTeamAsync();
			context.Wait(MessageReceivedAsync);
		}

		async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var activity = await result;

			var name = activity.Text;

			if(name.Contains("exit"))
			{
				context.Done("User exited");
				return;
			}

			var teamMember = _teamList.FirstOrDefault(m => m.Name.ToLowerInvariant().Contains(name) || m.Email.ToLowerInvariant().Contains(name));

			if(teamMember != null)
			{
				await context.PostAsync($"I found {teamMember.Name}. This is what {teamMember.Name} is doing: {teamMember.Description}");
			}
			else
			{
				await context.PostAsync("Sorry, I don't know this. Are you looking for a team member? Just tell me the name.");
			}

			context.Wait(MessageReceivedAsync);
		}
	}
}