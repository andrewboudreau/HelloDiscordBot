using Azure.Storage.Blobs;

namespace DiscordBotHost.Storage
{
	public class BlobStorage
	{
		private readonly BlobContainerClient containerClient;

		public BlobStorage(BlobContainerClient containerClient)
		{
			this.containerClient = containerClient;
			this.containerClient.CreateIfNotExistsAsync();
		}

		public async Task Read(string path, Action<BinaryData> content)
		{
			var response = await containerClient.GetBlobClient(path).DownloadContentAsync();
			content(response.Value.Content);
		}

		public async Task Save(string path, BinaryData binaryData)
		{
			await containerClient.UploadBlobAsync(path, binaryData);
		}

		protected virtual string ContentPath(string source, int id)
		{
			return PathForTextContent(source, id);
		}

		public static string PathForTextContent(string source, int id)
		{
			return $"{source.ToLower()}-{id}.txt";
		}
	}
}
