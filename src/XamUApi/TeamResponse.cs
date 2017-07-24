using System;
using System.Collections.Generic;
using System.Text;

namespace XamUApi
{
	[Serializable]
	public enum TeamMemberType
	{
		None = 0,
		Instructor = 1,
		Curriculum = 2,
		Support = 3,
		Engineering = 4,
		GuestLecturer = 5,
		TrainingPartner = 6
	}

	[Serializable]
	public class TeamResponse
	{
		public string Name { get; set; }
		public string Title { get; set; }
		public string Email { get; set; }
		public string TwitterHandle { get; set; }
		public string LinkedIn { get; set; }
		public string Website { get; set; }
		public string Icon { get; set; }
		public string Timezone { get; set; }
		public string Description { get; set; }
		public string ShortDescription { get; set; }
		public TeamMemberType Type { get; set; }
        public string HeadshotUrl
        {
            // cleanup dodgy URL's that are currentlybeing returned
            get { return Icon.Replace("https://university.xamarin.com/images/headshots/", ""); }
        }
        public string TwitterUrl
        {
            get { return $"https://twitter.com/{TwitterHandle}"; }
        }
    }
}
