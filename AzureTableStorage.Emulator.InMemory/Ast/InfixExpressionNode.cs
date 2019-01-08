namespace AzureTableStorage.Emulator.InMemory.Ast
{
	/// <summary>
	/// Expression node representing a an operation between two other nodes
	/// </summary>
	public class InfixExpressionNode : ExpressionNode
	{
		/// <summary>
		/// Gets or sets the verb
		/// </summary>
		public string Verb { get; set; }

		/// <summary>
		/// Gets or sets the left <see cref="ExpressionNode"/>
		/// </summary>
		public ExpressionNode Left { get; set; }

		/// <summary>
		/// Gets or sets the right <see cref="ExpressionNode"/>
		/// </summary>
		public ExpressionNode Right { get; set; }
	}
}
