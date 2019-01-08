using Antlr4.Runtime.Misc;
using AzureTableStorage.Emulator.InMemory.Ast;

namespace AzureTableStorage.Emulator.InMemory
{
	/// <summary>
	/// Grammar visitor implementation
	/// </summary>
	public class QueryFilterVisitor : QueryFilterBaseVisitor<ExpressionNode>
	{
		/// <summary>
		/// Visit Operation node
		/// </summary>
		public override ExpressionNode VisitOp([NotNull] QueryFilterParser.OpContext context)
		{
			var column = context.COLUMN();
			var target = context.TARGET();
			var verb = context.VERB();

			var node = new InfixExpressionNode()
			{
				Verb = verb.GetText(),
				Left = new ColumnNode()
				{
					Value = column.GetText()
				},
				Right = new TargetNode()
				{
					Value = target.GetText()
				}
			};

			return node;
		}

		/// <summary>
		/// Visit Symmetric node
		/// </summary>
		public override ExpressionNode VisitSymmetric([NotNull] QueryFilterParser.SymmetricContext context)
		{
			if (context.LBRACKET() != null)
			{
				return Visit(context.op());
			}

			var verb = context.VERB().GetText();
			var left = Visit(context.nestedQuery()[0].query());
			var right = Visit(context.nestedQuery()[1].query());

			return new InfixExpressionNode()
			{
				Verb = verb,
				Left = left,
				Right = right
			};
		}

		/// <summary>
		/// Visit left recursive node
		/// </summary>
		public override ExpressionNode VisitLeftRecursive([NotNull] QueryFilterParser.LeftRecursiveContext context)
		{
			return new InfixExpressionNode()
			{
				Verb = context.VERB().GetText(),
				Left = Visit(context.nestedQuery().query()),
				Right = VisitOp(context.op())
			};
		}

		/// <summary>
		/// Visit right recursive node
		/// </summary>
		public override ExpressionNode VisitRightRecursive([NotNull] QueryFilterParser.RightRecursiveContext context)
		{
			return new InfixExpressionNode()
			{
				Verb = context.VERB().GetText(),
				Left = VisitOp(context.op()),
				Right = Visit(context.nestedQuery().query())
			};
		}
	}
}
