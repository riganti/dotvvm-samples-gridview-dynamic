using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;

namespace DynamicGridViewSample.Services
{
    public class DynamicRowBinding
    {
        
        private static readonly ConcurrentDictionary<(DataContextStack dataContextStack, Type columnType, string columnName), IValueBinding> cache = new();

        public static IValueBinding GetOrCreate(DataContextStack rowDataContextStack, DbColumn column, BindingCompilationService bindingCompilationService)
        {
            return cache.GetOrAdd(
                (rowDataContextStack, column.DataType!, column.ColumnName),
                k => ValueBindingExpression.CreateBinding(bindingCompilationService, GetColumnBindingExpression(k.columnType, k.columnName), k.dataContextStack));
        }

        private static Expression<Func<object[], object>> GetColumnBindingExpression(Type columnType, string columnName)
        {
            if (DynamicRow.IsInt(columnType))
            {
                return h => ((DynamicRow)h[0]).Int[columnName];
            }
            else if (DynamicRow.IsDecimal(columnType))
            {
                return h => ((DynamicRow)h[0]).Decimal[columnName];
            }
            else if (DynamicRow.IsDateTime(columnType))
            {
                return h => ((DynamicRow)h[0]).DateTime[columnName];
            }
            else if (DynamicRow.IsBool(columnType))
            {
                return h => ((DynamicRow)h[0]).Bool[columnName];
            }
            else if (DynamicRow.IsString(columnType))
            {
                return h => ((DynamicRow)h[0]).String[columnName];
            }
            else
            {
                throw new NotSupportedException();
            }
        }

    }
}
