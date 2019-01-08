using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureTableStorage.Emulator.InMemory.Ast
{
	/// <summary>
	/// AST visitor responsible for traversing expression nodes
	/// </summary>
	/// <typeparam name="T">The table entity type</typeparam>
	public class AstVisitor<T>
		where T : ITableEntity, new()
	{
		/// <summary>
		/// Recursively visit all nodes, starting with a root infix node
		/// </summary>
		/// <param name="node">The root <see cref="InfixExpressionNode"/></param>
		/// <param name="table">The target in memory table</param>
		/// <returns>A result table</returns>
		public Dictionary<string, IDictionary<string, object>> VisitInfix(InfixExpressionNode node, IDictionary<string, IDictionary<string, object>> table)
		{
			var op = node.Verb;

			if (node.Left is InfixExpressionNode leftInfix && node.Right is InfixExpressionNode rightInfix)
			{
				var leftInfixResult = VisitInfix(leftInfix, table);
				var rightInfixResult = VisitInfix(rightInfix, table);

				var result = default(IEnumerable<KeyValuePair<string, IDictionary<string, object>>>);

				switch (op)
				{
					case "and":
						result = IntersectPartitions(leftInfixResult, rightInfixResult);
						break;
					case "not":
						// TODO not sure what this does
						break;
					case "or":
						result = UnionPartitions(leftInfixResult, rightInfixResult);
						break;
					default:
						break;
				}

				return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
			else
			{
				var column = node.Left as ColumnNode;
				var target = (node.Right as TargetNode).Value.Trim('\'');

				return ProcessQueryComparisonOp(table, op, column.Value, target)
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
		}

		private IEnumerable<KeyValuePair<string, IDictionary<string, object>>> ProcessQueryComparisonOp(IDictionary<string, IDictionary<string, object>> table, string op, string column, string target)
		{
			if (column == "PartitionKey")
			{
				var result = default(IEnumerable<KeyValuePair<string, IDictionary<string, object>>>);

				switch (op)
				{
					case "eq":
						result = table.Where(kvp => kvp.Key.Equals(target, StringComparison.InvariantCulture));
						break;
					case "ne":
						result = table.Where(kvp => !kvp.Key.Equals(target, StringComparison.InvariantCulture));
						break;
					default:
						break;
				}

				return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
			else if (column == "RowKey")
			{
				Func<KeyValuePair<string, object>, bool> predicate = default;

				switch (op)
				{
					case "eq":
						predicate = kvp => kvp.Key.Equals(target, StringComparison.InvariantCulture);
						break;
					case "ne":
						predicate = kvp => !kvp.Key.Equals(target, StringComparison.InvariantCulture);
						break;
					default:
						break;
				}

				var copy = new Dictionary<string, IDictionary<string, object>>();
				foreach (var key in table.Keys)
				{
					var filtered = table[key].Where(predicate).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

					if (filtered.Any())
					{
						copy[key] = filtered;
					}
					else
					{
						continue;
					}
				}

				return copy;
			}
			else
			{
				// TODO other column values, typed objects
				return table;
			}
		}

		private IEnumerable<KeyValuePair<string, IDictionary<string, object>>> IntersectPartitions(
			IEnumerable<KeyValuePair<string, IDictionary<string, object>>> left,
			IEnumerable<KeyValuePair<string, IDictionary<string, object>>> right)
		{
			return left.Intersect(right, new PartitionEqualityComparer());
		}

		private IEnumerable<KeyValuePair<string, IDictionary<string, object>>> UnionPartitions(
			IEnumerable<KeyValuePair<string, IDictionary<string, object>>> left,
			IEnumerable<KeyValuePair<string, IDictionary<string, object>>> right)
		{
			return left.Union(right, new PartitionEqualityComparer());
		}

		private class PartitionEqualityComparer : IEqualityComparer<KeyValuePair<string, IDictionary<string, object>>>
		{
			public bool Equals(KeyValuePair<string, IDictionary<string, object>> x, KeyValuePair<string, IDictionary<string, object>> y)
			{
				return x.Key.Equals(y.Key, StringComparison.InvariantCulture);
			}

			public int GetHashCode(KeyValuePair<string, IDictionary<string, object>> obj)
			{
				return obj.Key.GetHashCode();
			}
		}
	}
}
