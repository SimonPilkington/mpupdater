using System;
using System.Threading.Tasks;

namespace mpupdater
{
	public interface IDownloader
	{
		Task<byte[]> DownloadDataAsync();
		Task<byte[]> DownloadDataAsync(IProgress<double> progressReportCallback);

		Task DownloadFileAsync();
		Task DownloadFileAsync(string destination);
		Task DownloadFileAsync(IProgress<double> progressReportCallback);
		Task DownloadFileAsync(string destination, IProgress<double> progressReportCallback);
	}
}
