using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using AzureTableStorage.Emulator.InMemory.Ast;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace AzureTableStorage.Emulator.InMemory.Tests
{
	public class InMemoryTableStorageTests
	{
		private QueryFilterParser.QueryContext GetContext(string text)
		{
			var inputStream = new AntlrInputStream(text);
			var lexer = new QueryFilterLexer(inputStream);
			var commonTokenStream = new CommonTokenStream(lexer);
			var parser = new QueryFilterParser(commonTokenStream);
			return parser.query();
		}

		private IDictionary<string, IDictionary<string, object>> GetSimpleTable()
		{
			return new Dictionary<string, IDictionary<string, object>>
			{
				["partitionkey1"] = new Dictionary<string, object>()
				{
					["rowkey1"] = new MyEntity() { String = "partition1-row1" },
					["rowkey2"] = new MyEntity() { String = "partition1-row2" },
					["rowkey3"] = new MyEntity() { String = "partition1-row3" }
				},
				["partitionkey2"] = new Dictionary<string, object>()
				{
					["rowkey1"] = new MyEntity() { String = "partition2-row2" },
					["rowkey2"] = new MyEntity() { String = "partition2-row2" }
				},
				["partitionkey3"] = new Dictionary<string, object>()
				{
					["rowkey1"] = new MyEntity() { String = "partition3-row1" }
				}
			};
		}

		private Dictionary<string, IDictionary<string, MyEntity>> GetSimpleEntityTable()
		{
			return new Dictionary<string, IDictionary<string, MyEntity>>
			{
				["partitionkey1"] = new Dictionary<string, MyEntity>()
				{
					["rowkey1"] = new MyEntity(),
				},
				["partitionkey2"] = new Dictionary<string, MyEntity>()
				{
					["rowkey1"] = new MyEntity(),
					["rowkey2"] = new MyEntity()
				}
			};
		}

		[Fact]
		public void ItShouldParseSimpleQuery()
		{
			var c = GetContext("(PartitionKey eq 'test')");

			var resultNode = new QueryFilterVisitor().Visit(c);
			resultNode.Should().BeOfType(typeof(InfixExpressionNode));
		}

		[Fact]
		public void ItShouldParseNormalQuery()
		{
			var c = GetContext("(PartitionKey eq 'test') and (IsDoNotDisturb eq true)");
			var visitor = new QueryFilterVisitor().Visit(c);
		}

		[Fact]
		public void ItShouldParseWhenPK_IsEmail()
		{
			var c = GetContext("(PartitionKey eq 'test@test.com') and(IsDoNotDisturb eq true)");
			var visitor = new QueryFilterVisitor().Visit(c);
		}

		[Fact]
		public void ItShouldParseComplexQuery()
		{
			var c = GetContext("((PartitionKey eq 'test') and (IsDoNotDisturb eq true)) and (PartitionKey eq 'random')");
			var visitor = new QueryFilterVisitor().Visit(c);
		}

		[Fact]
		public void ItShouldDoSimpleOps_SimplePK()
		{
			var c = GetContext("(PartitionKey eq 'partitionkey1')");
			var resultNode = new QueryFilterVisitor().Visit(c) as InfixExpressionNode;

			var astVisitor = new AstVisitor<MyEntity>();
			var resultingTable = astVisitor.VisitInfix(resultNode, GetSimpleTable());
			resultingTable.Should().ContainKeys("partitionkey1");

			c = GetContext("(PartitionKey ne 'partitionkey1')");
			resultNode = new QueryFilterVisitor().Visit(c) as InfixExpressionNode;

			astVisitor = new AstVisitor<MyEntity>();
			resultingTable = astVisitor.VisitInfix(resultNode, GetSimpleTable());
			resultingTable.Should().ContainKeys("partitionkey2", "partitionkey3");
		}

		[Fact]
		public void ItShouldDoSimpleOps_SimpleRK()
		{
			var c = GetContext("(RowKey eq 'rowkey1')");
			var resultNode = new QueryFilterVisitor().Visit(c) as InfixExpressionNode;

			var astVisitor = new AstVisitor<MyEntity>();
			var resultingTable = astVisitor.VisitInfix(resultNode, GetSimpleTable());
			resultingTable.Should().ContainKeys("partitionkey1", "partitionkey2", "partitionkey3");

			c = GetContext("(RowKey ne 'rowkey1')");
			resultNode = new QueryFilterVisitor().Visit(c) as InfixExpressionNode;

			astVisitor = new AstVisitor<MyEntity>();
			resultingTable = astVisitor.VisitInfix(resultNode, GetSimpleTable());
			resultingTable.Should().ContainKeys("partitionkey1", "partitionkey2");
		}

		[Fact]
		public void ItShouldDoSimpleOps_AND()
		{
			var c = GetContext("(RowKey eq 'rowkey1') and (PartitionKey eq 'partitionkey1')");
			var resultNode = new QueryFilterVisitor().Visit(c) as InfixExpressionNode;

			var astVisitor = new AstVisitor<MyEntity>();
			var resultingTable = astVisitor.VisitInfix(resultNode, GetSimpleTable());
			resultingTable.Should().ContainKeys("partitionkey1");
			resultingTable["partitionkey1"].Should().ContainKeys("rowkey1");
		}

		[Fact]
		public void ItShouldDoSimpleOps_OR()
		{
			var c = GetContext("(RowKey eq 'rowkey1') or (PartitionKey eq 'partitionkey1')");
			var resultNode = new QueryFilterVisitor().Visit(c) as InfixExpressionNode;

			var astVisitor = new AstVisitor<MyEntity>();
			var resultingTable = astVisitor.VisitInfix(resultNode, GetSimpleTable());
			resultingTable.Should().ContainKeys("partitionkey1", "partitionkey2");
		}

		public class MyEntity : TableEntity
		{
			public string String { get; set; }

			public byte[] Bytes { get; set; }

			public bool Bool { get; set; }

			public DateTimeOffset DateTimeOffset { get; set; }

			public double Double { get; set; }

			public Guid Guid { get; set; }

			public int Int { get; set; }

			public long Long { get; set; }
		}
	}
}
