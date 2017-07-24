using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;
using System.Collections.Generic;
using System.Linq;

namespace XamUBot.Dialogs
{
	/// <summary>
	/// Allows querying the XamU team.
	/// </summary>
	[Serializable]
	public class TeamDialog : BaseDialog
	{
		IList<TeamResponse> _teamList;

		protected async override Task OnInitializeAsync(IDialogContext context)
		{
			await context.PostAsync("What would you like to know about our teams?");
			_teamList = await ApiManager.Instance.GetTeamAsync();
		}

		protected async override Task<bool> OnMessageReceivedAsync(IDialogContext context, Activity activity)
		{
			var keyword = activity.Text.ToLowerInvariant();

			// Presort list.
			IEnumerable<TeamResponse> teamMembers = _teamList
				.OrderBy(m => m.Name);

			// Show all members or filter.
			if (!keyword.Contains("all")
				&& !keyword.Contains("team")
				&& !keyword.Contains("every")
				&& !keyword.Contains("complete"))
			{
				// Filter content unless all team members have been requested.
				teamMembers = teamMembers
					.Where(
						m => m.Name.ToLowerInvariant().Contains(keyword)
						|| m.Email.ToLowerInvariant().Contains(keyword)
						|| m.Description.ToLowerInvariant().Contains(keyword)
						|| m.Title.ToLowerInvariant().Contains(keyword)
						|| m.TwitterUrl.ToLowerInvariant().Contains(keyword)
						|| m.Website.ToLowerInvariant().Contains(keyword));
			}

			var finalMembers = teamMembers.ToList();

			// Reply with a beautiful hero card!
			var reply = activity.CreateReply();

			if (finalMembers.Count > 0)
			{
				await context.PostAsync("Look what I found for you:");

				foreach (var person in finalMembers)
				{
					reply.AttachHeroCard(person.Name, person.Description, person.HeadshotUrl, person.ShortDescription, new Tuple<string, string>[] {
						new Tuple<string, string>("Twitter", person.TwitterHandle),
						new Tuple<string, string>("Email", person.Email)
					});
				}

				if (finalMembers.Count > 1)
				{
					reply.AttachmentLayout = "carousel";
				}
			}
			else
			{
				reply.AttachHeroCard(
					"Hello, is it me you're looking for?",
					"Sorry, I couldn't find who you were looking for. Are you looking for a team member? Just tell me the name of the team member or provide any other keyword.",
					@"http://i.imgur.com/QsIsjo8.jpg");
			}

			await context.PostAsync(reply);

			//await context.PostAsync("Is there anything else you would like to know about the team?");

			SendPicker(context, 1, "Is there anything else you would like to know about the team?");

			return false;
		}

		protected override Task<bool> OnPickerSelectedAsync(IDialogContext context, int pickerId, string selectedChoice)
		{
			if(selectedChoice == BaseDialog.ChoiceNo)
			{
				context.Done("Returning from team");
				return Task.FromResult(false);
			}
			else
			{
				return Task.FromResult(true);
			}
		}
	}
}