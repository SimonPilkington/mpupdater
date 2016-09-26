using System.Net;

namespace mpupdater
{
	public static class SpoofedWebClient
	{
		// Some hosts don't like weird user agents, so pretend we're IE11.
		public const string SpoofedUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
		public static WebClient Create()
		{
			var result = new WebClient();

			result.Headers.Add("user-agent", SpoofedUserAgent);
			return result;
		}
	}
}
