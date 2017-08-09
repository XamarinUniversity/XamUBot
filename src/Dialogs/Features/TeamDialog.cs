using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using XamUApi;
using System.Collections.Generic;
using System.Linq;
using QnAMakerDialog;
using System.Threading;

namespace XamUBot.Dialogs
{
	/// <summary>
	/// Allows querying the XamU team.
	/// Uses LUIS to get user's intent.
	/// </summary>
	[Serializable]
	public class TeamDialog : BaseDialog
	{
		IList<TeamResponse> _teamList;

		protected async override Task OnInitializeAsync(IDialogContext context)
		{
			await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.TeamWelcome));
			_teamList = await ApiManagerFactory.Instance.GetTeamAsync();
			WaitForNextMessage(context);
		}

		protected async override Task OnHelpReceivedAsync(IDialogContext context, Activity activity)
		{
			await context.PostAsync(ResponseUtterances.GetResponse(ResponseUtterances.ReplyTypes.TeamHelp));
			WaitForNextMessage(context);
		}

		bool _askedIfLookingForAnythingElse;

		protected async override Task OnMessageReceivedAsync(IDialogContext context, Activity activity)
		{
			var result = await LuisHelper.PredictLuisAsync(activity.Text, LuisConstants.IntentPrefix_Team);

			// Check if the last output was about asking the user if there's anything else.
			//TODO: This might be a  candidate for the "BestMatchDialog" (https://github.com/garypretty/botframework/tree/master/BestMatchDialog)
			if (_askedIfLookingForAnythingElse)
			{
				_askedIfLookingForAnythingElse = false;
				if (activity.Text.Trim().ToLowerInvariant() == "no"
					|| activity.Text.Trim().ToLowerInvariant() == "done"
					|| activity.Text.Trim().ToLowerInvariant() == "nope")
				{
					await context.PostAsync("Alright, what else should we talk about?");
					PopDialog(context);
					return;
				}
			}

			if (result?.TopScoringIntent == null)
			{
				// Check Q&A if we don't have an answer from LUIS. This will forward to the Q&A dialog and make it
				// return immediately.
				await CheckFaqAsync(context, activity);
				return;
			}

			// We have an answer from LUIS!
			var reply = activity.CreateReply();

			// Presort list.
			IEnumerable<TeamResponse> teamMembers = _teamList
				.OrderBy(m => m.Name);

			switch (result.TopScoringIntent.Name)
			{
				case LuisConstants.Intent_GetSpecialist:
					// The intent contains the technology the user is looking for.
					var technologyEntity = result.GetBestMatchingEntity(LuisConstants.Entity_Technology);

					await context.PostAsync($"Let me find some team members who know about '{technologyEntity.Name}'");

					teamMembers = teamMembers
						.Where(m =>
						   m.Description.ToLowerInvariant().Contains(technologyEntity.Name.ToLowerInvariant())
						   || m.Title.ToLowerInvariant().Contains(technologyEntity.Name.ToLowerInvariant()));
					break;

				case LuisConstants.Intent_ShowTeamMember:
					// The intent contains the technology the user is looking for.
					var personEntity = result.GetBestMatchingEntity(LuisConstants.Entity_Trainer);

					await context.PostAsync($"I'm looking for team members named '{personEntity.Name}'");

					teamMembers = teamMembers
						.Where(m =>
						   m.Name.ToLowerInvariant().Contains(personEntity.Name.ToLowerInvariant())
						   || m.Email.ToLowerInvariant().Contains(personEntity.Name.ToLowerInvariant()));
					break;

				case LuisConstants.Intent_ShowEntireTeam:
					// No further filtering.
					await context.PostAsync("Here's the entire team!");
					break;
			}

			var finalMembers = teamMembers.ToList();

			if (finalMembers.Count > 0)
			{
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

			_askedIfLookingForAnythingElse = true;
			await context.PostAsync("Anything else you'd like to know about the team?");


			WaitForNextMessage(context);
		}
	}
}