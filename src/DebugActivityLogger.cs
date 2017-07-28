using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using XamUApi;

namespace XamUBot
{
    public class DebugActivityLogger : IActivityLogger
    {
        public Task LogAsync(IActivity activity)
        {
            var auditItem = new BotAuditItem()
            {
                TimestampUtc = activity.Timestamp,
                SenderId = activity.From.Id,
                SenderName = activity.From.Name,
                RecipientId = activity.Recipient.Id,
                RecipientName = activity.Recipient.Name,
                //auditItem.UserId = ??? -- no idea how to get user id
                ConversationId = activity.Conversation.Id,
                ReplyToId = activity.ReplyToId,
                MessageId = activity.Id
            };
            StringBuilder message = new StringBuilder();

            // now try and fill in the message
            var msg = activity.AsMessageActivity();
            if (msg is Activity)
            {
                Activity a = msg as Activity;
                foreach (var attachment in a.Attachments)
                {
                    if (attachment.Content is HeroCard)
                    {
                        var card = attachment.Content as HeroCard;
                        message.AppendLine($"Sent HeroCard: {card.Text}");
                    }
                    else
                    {
                        message.AppendLine ($"Sent Attachment: {attachment.ContentType}");
                    }
                }

                if (!String.IsNullOrEmpty(a.Text))
                    message.AppendLine($"{a.Text}");

            }
            else
            {
                message.AppendLine($"*** {activity.AsMessageActivity()?.Text}");
            }

            auditItem.Message = message.ToString();

            Debug.WriteLine(auditItem);
            // maybe in a real one we would
            //return ApiManagerFactory.Instance.SaveAuditAsync(auditItem);
            return Task.CompletedTask;
        }
    }
}