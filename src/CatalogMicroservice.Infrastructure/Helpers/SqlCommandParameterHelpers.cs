using System.Data;
using Microsoft.Data.SqlClient;

namespace CatalogMicroservice.Infrastructure.Helpers;

public static class SqlCommandParameterHelpers
{
    public static void AddParameterValue(this SqlCommand @this, string name, SqlDbType type, object? value)
    {
        @this.Parameters.Add(new SqlParameter(name, type));
        @this.Parameters[name].Value = value;
    }
}
