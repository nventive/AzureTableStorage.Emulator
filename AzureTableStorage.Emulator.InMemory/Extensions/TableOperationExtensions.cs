using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureTableStorage.Emulator.InMemory.Extensions
{
	/// <summary>
	/// Table operation extensions
	/// </summary>
	public static class TableOperationExtensions
	{
		/// <summary>
		/// Retrieve the partition key from <see cref="TableOperation"/>
		/// </summary>
		/// <param name="table">The target <see cref="TableOperation"/></param>
		/// <returns>The partition key</returns>
		public static string PartitionKey(this TableOperation table)
		{
			var p = typeof(TableOperation)
				.GetProperty("PartitionKey", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

			return (string)p.GetValue(table, null);
		}

		/// <summary>
		/// Retrieve the partition key from <see cref="TableOperation"/>
		/// </summary>
		/// <param name="table">The target <see cref="TableOperation"/></param>
		/// <returns>The row key</returns>
		public static string RowKey(this TableOperation table)
		{
			var p = typeof(TableOperation)
				.GetProperty("RowKey", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

			return (string)p.GetValue(table, null);
		}

		/// <summary>
		/// Retrieve the partition key from <see cref="TableOperation"/>
		/// </summary>
		/// <param name="table">The target <see cref="TableOperation"/></param>
		/// <returns>The etag</returns>
		public static string Etag(this TableOperation table)
		{
			var p = typeof(TableOperation)
				.GetProperty("ETag", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

			return (string)p.GetValue(table, null);
		}
	}
}
