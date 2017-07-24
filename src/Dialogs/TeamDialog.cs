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

			var name = activity.Text.ToLowerInvariant();

			if(name.Contains("exit"))
			{
				context.Done("User exited");
				return;
			}

            var teamMembers =_teamList.Where(m => m.Name.ToLowerInvariant().Contains(name) || m.Email.ToLowerInvariant().Contains(name));

            if (teamMembers.Count() >= 1)
			{

                var message = context.MakeMessage();

                foreach (var person in teamMembers)
                {
                    message.Attachments.Add(CreateHeroCard(person).ToAttachment());
                }

                if (teamMembers.Count() > 1)
                    message.AttachmentLayout = "carousel";

                await context.PostAsync(message);
            }
			else
			{
				await context.PostAsync("Sorry, I couldn't find who you were looking for. Are you looking for a team member? Just tell me the name.");

                var msg = context.MakeMessage();
                msg.Attachments.Add(new Attachment()
                {
                    ContentUrl = @"http://i.imgur.com/QsIsjo8.jpg",
                    ContentType = "image/jpg",
                    Name = "Hello, is it me you're looking for"
                });
                await context.PostAsync(msg);

            }

			context.Wait(MessageReceivedAsync);
		}

        private HeroCard CreateHeroCard(TeamResponse teamMember)
        {
            var heroCard = new HeroCard
            {
                Title = teamMember.Name,
                Subtitle = teamMember.Title,
                Text = teamMember.ShortDescription,
                Images = new List<CardImage> { new CardImage(teamMember.HeadshotUrl) },
            };

            // create buttons as appropriate
            var buttons = new List<CardAction>();

            if (!string.IsNullOrEmpty(teamMember.TwitterHandle))
                buttons.Add(new CardAction(ActionTypes.OpenUrl, "Twitter Profile", value: teamMember.TwitterUrl));

            if (!string.IsNullOrEmpty(teamMember.Website))
                buttons.Add(new CardAction(ActionTypes.OpenUrl, "Website", value: teamMember.Website));

            heroCard.Buttons = buttons;

            return heroCard;
        }
    }
}