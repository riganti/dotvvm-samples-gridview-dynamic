using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DynamicGridViewSample.Services;

namespace DynamicGridViewSample.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {
        private readonly NorthwindService northwindService;
        private readonly BindingCompilationService bindingCompilationService;

        public GridViewDataSet<DynamicRow> Products { get; set; } = new()
        {
            PagingOptions =
            {
                PageSize = 250
            },
            SortingOptions =
            {
                SortExpression = "ProductID"
            }
        };

        public DefaultViewModel(NorthwindService northwindService, BindingCompilationService bindingCompilationService)
        {
            this.northwindService = northwindService;
            this.bindingCompilationService = bindingCompilationService;
        }

        public override async Task Load()
        {
            // we have to load data on every request in the Load phase
            // that's because the schema of the table is retrieved from the result of the query
            // and we need it to correctly build the GridView columns
            await GenerateColumns();

            await base.PreRender();
        }

        public override async Task PreRender()
        {
            // if the sort order was changed, we should reload the data
            if (Products.IsRefreshRequired)
            {
                await northwindService.LoadProducts(Products);
            }

            await base.PreRender();
        }

        private async Task GenerateColumns()
        {
            var schema = await northwindService.LoadProducts(Products);

            var gridView = (GridView)Context.View.FindControlByClientId("grid");

            // the first column is needed in order to retrieve DataContextStack for the row
            // if you don't need it, set Visible to false
            var rowDataContextStack = gridView.Columns[0].GetDataContextType();

            foreach (var column in schema)
            {
                // the bindings are cached because their creation on every HTTP request would be too expensive and would cause memory leaks
                var valueBinding = DynamicRowBinding.GetOrCreate(rowDataContextStack, column, bindingCompilationService);

                GridViewColumn gridColumn;
                if (column.ColumnName == "CategoryID")
                {
                    // hyperlink column
                    gridColumn = new GridViewTemplateColumn()
                    {
                        HeaderText = column.ColumnName,
                        AllowSorting = true,
                        SortExpression = column.ColumnName,
                        ContentTemplate = new DelegateTemplate(_ =>
                            new RouteLink() { RouteName = "Category" }
                                .SetProperty(RouteLink.TextProperty, ValueOrBinding<object>.FromBoxedValue(valueBinding).Select(v => "Category " + v))
                                .SetProperty(RouteLink.ParamsGroupDescriptor.GetDotvvmProperty("id"), valueBinding))
                    };
                }
                else
                {
                    // normal column
                    gridColumn = new GridViewTextColumn()
                    {
                        HeaderText = column.ColumnName,
                        AllowSorting = true,
                        SortExpression = column.ColumnName,
                        ValueBinding = valueBinding
                    };
                }

                gridView.Columns.Add(gridColumn);
            }
        }
    }
}
