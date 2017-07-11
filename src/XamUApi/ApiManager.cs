using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamUApi
{
    public class ApiManager
    {
		static Lazy<ApiManager> _instance = new Lazy<ApiManager>(() => new ApiManager(), false);

		public static ApiManager Instance => _instance.Value;


		HttpClient _client = new HttpClient
		{
			BaseAddress = new Uri("https://university.xamarin.com/api/v2/"),
			Timeout = TimeSpan.FromSeconds(30)
		};

		public async Task<IList<Track>> GetTracksAsync(string filter = null)
		{
			// It is also possible to add a filter; let's get all tracks containing "Azure". 

			var json = await _client.GetStringAsync(string.IsNullOrWhiteSpace(filter) ? "tracks" : $"tracks?Text={filter}");
			var tracks = JsonConvert.DeserializeObject<List<Track>>(json);
			return tracks;
		}

		public async Task<IList<TeamResponse>> GetTeamAsync()
		{
			// It is also possible to add a filter; let's get all tracks containing "Azure". 

			var json = await _client.GetStringAsync("teams");
			var team = JsonConvert.DeserializeObject<List<TeamResponse>>(json);
			return team;
		}

	}
}
