namespace AzureTableStorage.Emulator.InMemory.Ast
{
	/// <summary>
	/// Expression node representing a table column
	/// </summary>
	public class ColumnNode : ExpressionNode
	{
		/// <summary>
		/// Gets or sets the column value
		/// </summary>
		public string Value { get; set; }
	}
}
