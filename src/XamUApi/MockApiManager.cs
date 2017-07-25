using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamUApi
{
	public sealed class MockApiManager : IApiManager
	{
		const int NetworkDelayMilliseconds = 500;
		public async Task<IList<TeamResponse>> GetTeamAsync()
		{
			// Simulate some network delay.
			await Task.Delay(NetworkDelayMilliseconds);

			return new List<TeamResponse>
			{
				new TeamResponse
				{
					Name = "Kym",
					Description = "Kym is one of the Australian guys and knows pretty much everything.",
					Icon = @"http://i.imgur.com/QsIsjo8.jpg"
				},

				new TeamResponse
				{
					Name = "René",
					Description = "René really only knows Turbo Pascal but pretends to be an iOS expert.",
					Icon = @"http://i.imgur.com/QsIsjo8.jpg"
				}
			};
		}

		public async Task<IList<Track>> GetTracksAsync(string filter = null)
		{
			// Simulate some network delay.
			await Task.Delay(NetworkDelayMilliseconds);

			return new List<Track>
			{
				new Track
				{
					Id = 1,
					Slug = "IOS",
					Description = "Learn all about Apple's iPad and iPhone"
				},

				new Track
				{
					Id = 2,
					Slug = "AND",
					Description = "Learn all about Android by Google"
				},
			};
		}

		public async Task<BotAuditItem> SaveAuditAsync(BotAuditItem auditItem)
		{
			if(auditItem == null)
			{
				return null;
			}

			auditItem.TimestampUtc = DateTime.UtcNow;
			_mockedAudit.Add(auditItem);
			return auditItem;
		}

		static List<BotAuditItem> _mockedAudit = new List<BotAuditItem>();

		public IEnumerable<BotAuditItem> MockAudit => _mockedAudit;
	}
}
