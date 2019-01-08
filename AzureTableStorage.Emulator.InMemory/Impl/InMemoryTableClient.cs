using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureTableStorage.Emulator.InMemory.Impl
{
	/// <summary>
	/// The in memory table collection holder
	/// </summary>
	public class InMemoryTableClient : CloudTableClient
	{
		private IDictionary<string, InMemoryTable> _inMemoryTables;

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryTableClient"/> class.
		/// </summary>
		public InMemoryTableClient()
			: this(new Uri("https://inmemory.com/inmemory"), null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryTableClient"/> class.
		/// </summary>
		public InMemoryTableClient(Uri baseUri, StorageCredentials credentials)
			: base(baseUri, credentials)
		{
			_inMemoryTables = new Dictionary<string, InMemoryTable>();
		}

		/// <summary>
		/// Get a <see cref="InMemoryTable"/>, creating a new one if it does not exist
		/// </summary>
		/// <param name="name">The table name</param>
		/// <returns>A <see cref="InMemoryTable"/></returns>
		public override CloudTable GetTableReference(string name)
		{
			return _inMemoryTables.GetOrCreate(name);
		}
	}
}
