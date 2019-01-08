using System.Collections.Generic;

namespace AzureTableStorage.Emulator.InMemory
{
	/// <summary>
	/// Dictionary extensions
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Get the value if it exists, or create, set and return it
		/// </summary>
		/// <typeparam name="T">The type of the key</typeparam>
		/// <typeparam name="U">The type of the value</typeparam>
		/// <param name="dict">The target dictionary</param>
		/// <param name="key">The target key</param>
		/// <returns><typeparamref name="U"/></returns>
		public static U GetOrCreate<T, U>(this IDictionary<T, U> dict, T key)
			where U : class, new()
		{
			if (dict.TryGetValue(key, out var value))
			{
				return value;
			}
			else
			{
				var u = new U();
				dict[key] = u;
				return u;
			}
		}
	}
}
