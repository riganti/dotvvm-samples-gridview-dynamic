using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DynamicGridViewSample.Services;

public class DynamicQueryService
{
    private readonly SqlConnection connection;

    public DynamicQueryService(SqlConnection connection)
    {
        this.connection = connection;
    }

    protected async Task<ReadOnlyCollection<DbColumn>> ExecuteDynamicQuery(GridViewDataSet<DynamicRow> dataSet, string query)
    {
        var sql = AppendSkipTakeOrderBy(query, dataSet.PagingOptions.PageIndex, dataSet.PagingOptions.PageSize, dataSet.SortingOptions.SortExpression, dataSet.SortingOptions.SortDescending);

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var schema = reader.GetColumnSchema();
        var records = new List<DynamicRow>();
        while (reader.HasRows && await reader.ReadAsync())
        {
            var record = new DynamicRow();
            for (var i = 0; i < schema.Count; i++)
            {
                var columnName = schema[i].ColumnName;
                var columnType = schema[i].DataType!;
                var value = reader[i];

                if (DBNull.Value == value)
                {
                    value = null;
                }

                record.Set(value, columnName, columnType);
            }

            records.Add(record);
        }
        await reader.CloseAsync();

        dataSet.Items = records;

        await using var command2 = new SqlCommand(AppendCount(query), connection);
        dataSet.PagingOptions.TotalItemsCount = (int)(await command2.ExecuteScalarAsync())!;

        dataSet.IsRefreshRequired = false;

        return schema;
    }

    private string AppendCount(string query)
    {
        return $"SELECT COUNT(*) FROM ({query}) AS [__innerQuery]";
    }

    private string AppendSkipTakeOrderBy(string query, int skip, int take, string orderByColumnName, bool orderByDescending)
    {
        if (!Regex.IsMatch(orderByColumnName, @"^[a-zA-Z_][a-zA-Z0-9_]+$"))
        {
            // make sure the column name doesn't contain any SQL chars - only letters and numbers and underscore are supported
            throw new SecurityException();
        }
        return query + $" ORDER BY [{orderByColumnName}] {(orderByDescending ? " DESC" : "")} OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
    }

}