using System.Collections.Generic;

namespace XamUApi
{
	/// <summary>
	/// The C# representation of a topic.
	/// The courses we offer are called topics. There are topics for iOS, Android etc.
	/// Topics are scheduled regularly and you’re able to register.
	/// Topics have a name, a description, a duration and more. A scheduled topic is called a “class” (see below).
	/// </summary>
	public class Topic
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string ShortDescription { get; set; }
		public string Slug { get; set; }
		public bool IsFeaturedClass { get; set; }
		public string Track { get; set; }
		public string VideoEmbedCode { get; set; }
		public int DurationInMinutes { get; set; }
		public List<ClassTime> ClassTimes { get; set; }
	}

	
}
