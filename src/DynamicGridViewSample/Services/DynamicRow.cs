using System;
using System.Collections.Generic;

namespace DynamicGridViewSample.Services;

public class DynamicRow
{
    public Dictionary<string, int?> Int { get; set; } = new();
    public Dictionary<string, decimal?> Decimal { get; set; } = new();
    public Dictionary<string, DateTime?> DateTime { get; set; } = new();
    public Dictionary<string, bool?> Bool { get; set; } = new();
    public Dictionary<string, string> String { get; set; } = new();

    public static bool IsInt(Type columnType) 
        => columnType == typeof(int) || columnType == typeof(short) 
        || columnType == typeof(int?) || columnType == typeof(short?);
    public static bool IsDecimal(Type columnType) 
        => columnType == typeof(decimal) || columnType == typeof(float) || columnType == typeof(double)
        || columnType == typeof(decimal?) || columnType == typeof(float?) || columnType == typeof(double?);
    public static bool IsDateTime(Type columnType) 
        => columnType == typeof(DateTime)
        || columnType == typeof(DateTime?);
    public static bool IsBool(Type columnType) 
        => columnType == typeof(bool)
        || columnType == typeof(bool?);
    public static bool IsString(Type columnType) 
        => columnType == typeof(string);

    public void Set(object value, string columnName, Type columnType)
    {
        if (columnType == typeof(int))
        {
            Int[columnName] = (int?)value;
        }
        else if (columnType == typeof(short))
        {
            // short can be treated as int
            Int[columnName] = (short?)value;
        }
        else if (columnType == typeof(decimal))
        {
            Decimal[columnName] = (decimal?)value;
        }
        else if (columnType == typeof(float))
        {
            // float can be treated as decimal
            Decimal[columnName] = (decimal?)(float?)value;
        }
        else if (columnType == typeof(double))
        {
            // double can be treated as decimal
            Decimal[columnName] = (decimal?)(double?)value;
        }
        else if (columnType == typeof(DateTime))
        {
            DateTime[columnName] = (DateTime?)value;
        }
        else if (columnType == typeof(bool))
        {
            Bool[columnName] = (bool?)value;
        }
        else if (columnType == typeof(string))
        {
            String[columnName] = (string)value;
        }
        else
        {
            throw new NotSupportedException($"Columns of type {columnType} are not supported!");
        }
    }
}