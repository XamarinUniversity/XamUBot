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
        public const string ApiEndpoint = "https://university.xamarin.com/api/v2/";
        HttpClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        public ApiManager()
        {
            _client = new HttpClient {
                BaseAddress = new Uri(ApiEndpoint),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Gets available tracks.
        /// </summary>
        /// <param name="filter">keyword to filter for. Can be NULL to return everything.</param>
        /// <returns>List of tracks</returns>
        public async Task<IList<Track>> GetTracksAsync(string filter = null)
		{
			var json = await _client.GetStringAsync(string.IsNullOrWhiteSpace(filter) ? "tracks" : $"tracks?Text={filter}");
			return JsonConvert.DeserializeObject<List<Track>>(json);
		}

		/// <summary>
		/// Gets the XamU team.
		/// </summary>
		/// <returns>list of team members</returns>
		public async Task<IList<TeamResponse>> GetTeamAsync()
		{
			var json = await _client.GetStringAsync("teams");
			return JsonConvert.DeserializeObject<List<TeamResponse>>(json);
		}

        /// <summary>
        /// Save the audit trail
        /// </summary>
        /// <param name="auditItem"></param>
        /// <returns></returns>
		public Task<BotAuditItem> SaveAuditAsync(BotAuditItem auditItem)
		{
            return (auditItem != null) 
                ? Task.FromResult(auditItem) 
                : null;
        }
	}
}
