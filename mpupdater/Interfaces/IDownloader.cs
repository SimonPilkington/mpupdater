using System;
using System.Threading.Tasks;

namespace mpupdater
{
	public interface IDownloader
	{
		byte[] DownloadData();
		void DownloadFile();
		void DownloadFile(string destination);

		Task<byte[]> DownloadDataAsync(IProgress<double> progressReportCallback);
		Task DownloadFileAsync(IProgress<double> progressReportCallback);
		Task DownloadFileAsync(string destination, IProgress<double> progressReportCallback);
	}
}
