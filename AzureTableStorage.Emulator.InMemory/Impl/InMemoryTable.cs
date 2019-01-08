using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using AzureTableStorage.Emulator.InMemory.Ast;
using AzureTableStorage.Emulator.InMemory.Extensions;
using AzureTableStorage.Emulator.InMemory.Factory;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureTableStorage.Emulator.InMemory.Impl
{
	/// <summary>
	/// An in memory table implementation
	/// </summary>
	public class InMemoryTable : CloudTable
	{
		private readonly IDictionary<string, IDictionary<string, object>> _table;

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryTable"/> class.
		/// </summary>
		public InMemoryTable()
			: this(new Uri("https://inmemory.com/inmemory"))
		{
			_table = new Dictionary<string, IDictionary<string, object>>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryTable"/> class.
		/// </summary>
		public InMemoryTable(Uri tableAddress)
			: base(tableAddress)
		{
		}

		/// <summary>
		/// Create the table if it does not exist
		/// </summary>
		public override async Task<bool> CreateIfNotExistsAsync()
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// In memory implementation of <see cref="ExecuteQuerySegmentedAsync{T}(TableQuery{T}, TableContinuationToken)"/>
		/// </summary>
		public override Task<TableQuerySegment<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token)
		{
			var inputStream = new AntlrInputStream(query.FilterString);
			var lexer = new QueryFilterLexer(inputStream);
			var commonTokenStream = new CommonTokenStream(lexer);
			var parser = new QueryFilterParser(commonTokenStream);
			var queryContext = parser.query();
			var resultNode = new QueryFilterVisitor().Visit(queryContext) as InfixExpressionNode;
			var astVisitor = new AstVisitor<T>();

			var result = astVisitor.VisitInfix(resultNode, _table);

			var resultList = new List<T>();
			foreach (var key in result.Keys)
			{
				resultList.AddRange(result[key].Select(kvp => kvp.Value).Cast<T>().ToList());
			}

			var resultSegment = TableQueryFactory.Construct(resultList);

			return Task.FromResult(TableQueryFactory.Construct(resultSegment));
		}

		/// <summary>
		/// In memory implementation of <see cref="ExecuteAsync(TableOperation)"/>
		/// </summary>
		public override Task<TableResult> ExecuteAsync(TableOperation operation)
		{
			var partitionKey = operation.PartitionKey();
			var rowKey = operation.RowKey();

			switch (operation.OperationType)
			{
				case TableOperationType.Retrieve:
					if (_table.TryGetValue(partitionKey, out var retrieveInner))
					{
						if (retrieveInner.TryGetValue(rowKey, out var retrieveResult))
						{
							return Task.FromResult(new TableResult()
							{
								Result = _table[partitionKey][rowKey]
							});
						}
					}

					return Task.FromResult(new TableResult());
				case TableOperationType.Insert:
				case TableOperationType.InsertOrReplace:
				case TableOperationType.InsertOrMerge:
					if (_table.TryGetValue(partitionKey, out var insertInner) && insertInner != null)
					{
						if (insertInner.TryGetValue(rowKey, out var insertCurrent) && insertCurrent != null)
						{
							if (operation.OperationType == TableOperationType.Insert)
							{
								throw new InvalidOperationException("Entity exists, call InserOrReplace or InsertOrMerge instead");
							}

							if (operation.OperationType == TableOperationType.InsertOrReplace)
							{
								_table[partitionKey][rowKey] = operation.Entity;
							}
							else
							{// TODO  merge
							}
						}
						else
						{
							insertInner[rowKey] = operation.Entity;
						}
					}
					else
					{
						_table[partitionKey] = new Dictionary<string, object>
						{
							[rowKey] = operation.Entity
						};
					}

					break;
				case TableOperationType.Delete:
					if (_table.TryGetValue(partitionKey, out var deleteInner))
					{
						if (deleteInner.TryGetValue(rowKey, out var deleteCurrent))
						{
							_table[partitionKey][rowKey] = null;
						}
					}

					break;
				default:
					break;
			}

			return Task.FromResult(new TableResult());
		}
	}
}
