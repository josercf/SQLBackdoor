using Dapper;

using Microsoft.AspNetCore.Mvc;

using SQLBackdoor.Models;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLBackdoor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SqlExecutorController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QueryData model)
        {

            try
            {
                var connectionString = $@"Data Source={model.Server};Initial Catalog={model.Database};User Id={model.Username};Password={model.Password};";

                if (!string.IsNullOrEmpty(model.Parameters))
                    connectionString += $"{model.Parameters}";

                var query = model.Query.Replace("&#039;", "'");

                using (var connection = new SqlConnection(connectionString))
                {
                    var ds = await connection.QueryMultipleAsync(query, commandTimeout: model.Timeout ?? 120);

                    //var ds = await connection.QueryAsync(query, commandTimeout: model.Timeout??120);

                    var response = new ExecutionResponse
                    {
                        Error = false,
                        Message = ""
                    };

                    IEnumerable<dynamic> result = null;
                    while(!ds.IsConsumed)
                    {
                        result = await ds.ReadAsync();
                        var (cols,data) = ReadColumns(result);
                        response.Results.Add(new ExecutionResponseResult
                        {
                            Columns = cols,
                            Data = data
                        });
                    }                  
                        

                    return Ok(response);
                }
            }
            catch (SqlException ex)
            {
                return Ok(new ExecutionResponse { Error = true, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        private (IEnumerable<ExecutionResponseResultColumn>,IEnumerable<dynamic>) ReadColumns(IEnumerable<dynamic> result)
        {
            if (!result.Any())
                return (new ExecutionResponseResultColumn[] { }, new dynamic[] { });

            var row = result.First();

            var dict = (IDictionary<string, object>)row;

            var cols = new List<ExecutionResponseResultColumn>();
            var data = new List<ExpandoObject>();
                        
            for(var i = 0; i < dict.Count; i++)
            {
                var key = dict.Keys.ElementAt(i);                
                var col = new ExecutionResponseResultColumn
                {
                    Name = $"column{i+1}",
                    Label = key
                };
                cols.Add(col);
            }

            var r = 1;
            foreach(var item in result)
            {
                var obj = (IDictionary<string, object>)item;                
                var expObj = new ExpandoObject();

                expObj.TryAdd("#", r);
                for(var i = 0; i < obj.Count; i++)
                {
                    var elem = obj.Values.ElementAt(i);
                    var colname = cols.ElementAt(i).Name;
                    expObj.TryAdd(colname, elem);
                }
                data.Add(expObj);
                r++;
            }

            cols.Insert(0,new ExecutionResponseResultColumn { Name = "#", Label = "#" });

            return (cols, data);
        }
    }

    public class QueryData
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Parameters { get; set; }
        public int? Timeout { get; set; }
        public string Query { get; set; }        
    }
}
