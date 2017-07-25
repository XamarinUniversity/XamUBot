using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamUApi
{
	/// <summary>
	/// Handles communication with the XamU web API.
	/// </summary>
    public sealed class ApiManager : IApiManager
	{
		HttpClient _client = new HttpClient
		{
			BaseAddress = new Uri("https://university.xamarin.com/api/v2/"),
			Timeout = TimeSpan.FromSeconds(30)
		};

		/// <summary>
		/// Gets available tracks.
		/// </summary>
		/// <param name="filter">keyword to filter for. Can be NULL to return everything.</param>
		/// <returns>List of tracks</returns>
		public async Task<IList<Track>> GetTracksAsync(string filter = null)
		{
			var json = await _client.GetStringAsync(string.IsNullOrWhiteSpace(filter) ? "tracks" : $"tracks?Text={filter}");
			var tracks = JsonConvert.DeserializeObject<List<Track>>(json);
			return tracks;
		}

		/// <summary>
		/// Gets the XamU team.
		/// </summary>
		/// <returns>list of team members</returns>
		public async Task<IList<TeamResponse>> GetTeamAsync()
		{
			var json = await _client.GetStringAsync("teams");
			var team = JsonConvert.DeserializeObject<List<TeamResponse>>(json);
			return team;
		}

		public async Task<BotAuditItem> SaveAuditAsync(BotAuditItem auditItem)
		{
			if(auditItem == null)
			{
				return null;
			}

			return auditItem;
		}

	}
}
