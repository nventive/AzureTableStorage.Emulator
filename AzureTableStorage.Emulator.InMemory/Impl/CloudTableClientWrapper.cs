using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureTableStorage.Emulator.InMemory.Impl
{
	/// <summary>
	/// Wrapper class that exposes to <see cref="InMemoryTableClient"/>
	/// </summary>
	public class CloudTableClientWrapper
	{
		private InMemoryTableClient _inMemory;
		private CloudTableClient _normal;

		private bool _useInMemoryStorage;

		/// <summary>
		/// Initializes a new instance of the <see cref="CloudTableClientWrapper"/> class.
		/// </summary>
		/// <param name="connectionString">The storage connection string. If null is passed, in memory storage is used</param>
		public CloudTableClientWrapper(string connectionString = null)
		{
			if (connectionString != null)
			{
				var storage = CloudStorageAccount.Parse(connectionString);
				_normal = storage.CreateCloudTableClient();
				_useInMemoryStorage = false;
			}
			else
			{
				_inMemory = new InMemoryTableClient();
				_useInMemoryStorage = true;
			}
		}

		/// <summary>
		/// Gets the default request options for the underlying <see cref="CloudTableClient"/>
		/// </summary>
		/// <remarks>In case of in memory storage, creates a new <see cref="TableRequestOptions"/> object</remarks>
		public TableRequestOptions DefaultRequestOptions => _normal?.DefaultRequestOptions ?? new TableRequestOptions();

		/// <summary>
		/// Get a <see cref="CloudTable"/> or a <see cref="InMemoryTable"/>
		/// </summary>
		/// <param name="tableName">The table name</param>
		/// <returns>A <see cref="CloudTable"/> or a <see cref="InMemoryTable"/></returns>
		public CloudTable GetTableReference(string tableName)
		{
			return _useInMemoryStorage
				? _inMemory.GetTableReference(tableName)
				: _normal.GetTableReference(tableName);
		}
	}
}
