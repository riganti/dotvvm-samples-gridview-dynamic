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

public class NorthwindService : DynamicQueryService
{

    public NorthwindService(SqlConnection connection) : base(connection)
    {
    }

    public async Task<ReadOnlyCollection<DbColumn>> LoadProducts(GridViewDataSet<DynamicRow> dataSet)
    {
        var query = "SELECT * FROM [Products]";
        return await ExecuteDynamicQuery(dataSet, query);
    }

}