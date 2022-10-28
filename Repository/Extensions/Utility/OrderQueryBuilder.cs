using Entities.Models;
using System.Reflection;
using System.Text;

namespace Repository.Extensions.Utility
{
    public static class OrderQueryBuilder
    {
		/*
		 * Example of request:
		 * https://localhost:5001/api/companies/companyId/employees?orderBy=name,age desc
		 * 
		 * orderByQueryString = name,age desc
		 */
		public static string CreateOrderQuery<T>(string orderByQueryString)
        {
			//split orderByQueryString by comma to retrieve parameters to order by
			var orderParams = orderByQueryString.Trim().Split(',');

			//retreive properties of type(T) the query is being built for
			var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var orderQueryBuilder = new StringBuilder();

			//loop through parameters given and check if they exist in the type(T)
			foreach (var param in orderParams)
			{
				if (string.IsNullOrWhiteSpace(param))
					continue;

				//extract properties from orderByQueryString
				//expectd result: string propertyFromQueryName ="name" "age" "desc"
				var propertyFromQueryName = param.Split(" ")[0];

				var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName,
					StringComparison.InvariantCultureIgnoreCase)); 
				//if property is not found then skip to the next param
				if (objectProperty == null)
					continue;

				var direction = param.EndsWith(" desc") ? "descending" : "ascending";
				orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {direction}, ");
			}

			return orderQueryBuilder.ToString().TrimEnd(',', ' ');
		}
    }
}
