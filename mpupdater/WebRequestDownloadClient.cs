using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace mpupdater
{
	/// <summary>
	/// Represents a single download operation.
	/// </summary>
	public class WebRequestDownloadClient : IDownloader
	{
		private const int CHUNK_SIZE = 65536;
		private readonly Uri resource;

		public double ProgressReportPercentageIncrement { get; set; }

		public WebRequestDownloadClient(string url)
		{
			resource = new Uri(url);
			ProgressReportPercentageIncrement = 0.15;
		}

		/// <summary>
		/// Download data from a web resource after the request has been sent.
		/// </summary>
		/// <param name="response">A WebResponse object.</param>
		/// <param name="outStream">The stream to write the data to.</param>
		/// <param name="progressReport">Object implementing IProgress to report the progress of an aync download operation. This can be null.</param>
		private async Task GetBytesFromResponse(WebResponse response, Stream outStream, IProgress<double> progressReport)
		{
			var responseStream = response.GetResponseStream();

			var downloadBuffer = new byte[CHUNK_SIZE];
			long totalBytesReceived = 0;

			int bytesPerProgressReport = (int)(response.ContentLength / (100 / ProgressReportPercentageIncrement));
			int progressCounter = 0;

			do
			{
				int bytesReceived = await responseStream.ReadAsync(downloadBuffer, 0, CHUNK_SIZE).ConfigureAwait(false);

				if (bytesReceived > 0)
				{
					await outStream.WriteAsync(downloadBuffer, 0, bytesReceived).ConfigureAwait(false);
					totalBytesReceived += bytesReceived;

					if (progressReport != null)
					{
						progressCounter += bytesReceived;

						if (progressCounter >= bytesPerProgressReport
							|| totalBytesReceived == response.ContentLength) // always report 100%
						{
							progressCounter = 0;
							double ratio = (double)totalBytesReceived / response.ContentLength;
							progressReport.Report(ratio * 100);
						}
					}
				}
			} while (totalBytesReceived < response.ContentLength);
		}
		
		public Task<byte[]> DownloadDataAsync() => DownloadDataAsync(null);

		public async Task<byte[]> DownloadDataAsync(IProgress<double> progressReportCallback)
		{
			var request = WebRequest.Create(resource);
			var response = await request.GetResponseAsync().ConfigureAwait(false);

			var outBytes = new byte[response.ContentLength];
			var destStream = new MemoryStream(outBytes);

			using (response)
			{
				using (destStream)
					await GetBytesFromResponse(response, destStream, progressReportCallback).ConfigureAwait(false);
			}

			return outBytes;
		}
		
		public Task DownloadFileAsync() => DownloadFileAsync(null, null);
		public Task DownloadFileAsync(IProgress<double> progressReportCallback) => DownloadFileAsync(null, progressReportCallback);
		public Task DownloadFileAsync(string destination) => DownloadFileAsync(destination, null);

		public async Task DownloadFileAsync(string destination, IProgress<double> progressReportCallback)
		{
			var request = WebRequest.Create(resource);
			var response = await request.GetResponseAsync().ConfigureAwait(false);
			var destStream = new FileStream(destination ?? Path.GetFileName(response.ResponseUri.LocalPath), FileMode.Create);

			using (response)
			{
				using (destStream)
					await GetBytesFromResponse(response, destStream, progressReportCallback).ConfigureAwait(false);
			}
		}
	}
}
