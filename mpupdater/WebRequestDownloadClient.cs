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
		private const int CHUNK_SIZE = 16384;

		Uri resource;

		public double ProgressReportPercentageIncrement { get; set; }

		public WebRequestDownloadClient(string url)
		{
			resource = new Uri(url);
			ProgressReportPercentageIncrement = 0.15;
		}

		private WebResponse SendRequest()
		{
			var request = WebRequest.Create(resource);
			var response = request.GetResponse();

			return response;
		}

		/// <summary>
		/// Download data from a web resource after the request has been sent.
		/// </summary>
		/// <param name="response">The response, returned by SendRequest.</param>
		/// <param name="outStream">The stream to write the data to.</param>
		/// <param name="progressReport">Object implementing IProgress to report the progress of an aync download operation. This parm can be null.</param>
		private void GetBytesFromResponse(WebResponse response, Stream outStream, IProgress<double> progressReport)
		{
			var responseStream = response.GetResponseStream();

			var downloadBuffer = new byte[CHUNK_SIZE];
			int totalBytesReceived = 0;

			int bytesPerProgressReport = (int)(response.ContentLength / (100 / ProgressReportPercentageIncrement));
			int progressCounter = 0;

			do
			{
				int bytesReceived = responseStream.Read(downloadBuffer, 0, CHUNK_SIZE);

				if (bytesReceived > 0)
				{
					outStream.Write(downloadBuffer, 0, bytesReceived);
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
		
		public byte[] DownloadData() => DownloadData(null);

		private byte[] DownloadData(IProgress<double> progressReportCallback)
		{
			var response = SendRequest();

			var outBytes = new byte[response.ContentLength];
			var destStream = new MemoryStream(outBytes);

			using (response)
				using (destStream)
					GetBytesFromResponse(response, destStream, progressReportCallback);

			return outBytes;
		}
		
		public void DownloadFile() => DownloadFile(null, null);
		public void DownloadFile(string destination) => DownloadFile(destination, null);

		private void DownloadFile(string destination, IProgress<double> progressReportCallback)
		{
			var response = SendRequest();

			string fileDestionation = destination ?? Path.GetFileName(response.ResponseUri.LocalPath);
			var destStream = new FileStream(fileDestionation, FileMode.Create);

			using (response)
				using (destStream)
					GetBytesFromResponse(response, destStream, progressReportCallback);
		}

		public Task<byte[]> DownloadDataAsync(IProgress<double> progressReportCallback)
		{
			return Task.Run(() => DownloadData(progressReportCallback));
		}

		public Task DownloadFileAsync(IProgress<double> progressReportCallback)
		{
			return Task.Run(() => DownloadFile(null, progressReportCallback));
		}

		public Task DownloadFileAsync(string destination, IProgress<double> progressReportCallback)
		{
			return Task.Run(() => DownloadFile(destination, progressReportCallback));
		}
	}
}
