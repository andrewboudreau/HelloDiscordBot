using Azure.Storage.Blobs;

namespace DiscordBotHost.Storage
{
	public class TextContentStore
	{
		private readonly BlobContainerClient containerClient;

		public TextContentStore(BlobContainerClient containerClient)
		{
			this.containerClient = containerClient;
			this.containerClient.CreateIfNotExistsAsync();
		}

		public async Task Read(string module, int id, Action<BinaryData> content)
		{
			var response = await containerClient.GetBlobClient(ContentPath(module, id)).DownloadContentAsync();
			content(response.Value.Content);
		}

		public async Task Save(string source, int id, BinaryData binaryData)
		{
			await containerClient.UploadBlobAsync(ContentPath(source, id), binaryData);
		}

		protected virtual string ContentPath(string module, int id)
		{
			return $"{module.ToLower()}-{id}.txt";
		}
	}
}
