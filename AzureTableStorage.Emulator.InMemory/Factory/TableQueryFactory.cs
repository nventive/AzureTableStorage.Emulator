using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureTableStorage.Emulator.InMemory.Factory
{
	/// <summary>
	/// Factory class to generate <see cref="TableQuerySegment{TElement}"/> and <see cref="ResultSegment{TElement}"/>
	/// </summary>
	public static class TableQueryFactory
	{
		/// <summary>
		/// Construct a <see cref="TableQuerySegment{TElement}"/> from a <see cref="ResultSegment{TElement}"/>
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="resultSegment">The result segment to use as constructor parameter</param>
		/// <returns>A <see cref="TableQuerySegment{TElement}"/></returns>
		public static TableQuerySegment<T> Construct<T>(ResultSegment<T> resultSegment)
		{
			return (TableQuerySegment<T>)typeof(TableQuerySegment<T>).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ResultSegment<T>) }, null)
				 .Invoke(new object[] { resultSegment });
		}

		/// <summary>
		/// Construct a <see cref="ResultSegment{TElement}"/> from a <see cref="List{TElement}"/>
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="result">The result list to use as constructor parameter</param>
		/// <returns>A <see cref="ResultSegment{TElement}"/></returns>
		public static ResultSegment<T> Construct<T>(List<T> result)
		{
			return (ResultSegment<T>)typeof(ResultSegment<T>).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(List<T>) }, null)
				 .Invoke(new object[] { result });
		}
	}
}
