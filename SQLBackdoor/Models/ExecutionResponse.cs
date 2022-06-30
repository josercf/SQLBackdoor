using System.Collections.Generic;
using System.Linq;

namespace SQLBackdoor.Models
{
    public class ExecutionResponse
    {
        public bool Error { get; set; }

        public string Message { get; set; }

        public IList<ExecutionResponseResult> Results { get; set; }

        public ExecutionResponse()
        {
            Results = new List<ExecutionResponseResult>();
        }
    }

    public class ExecutionResponseResult
    {
        public IEnumerable<ExecutionResponseResultColumn> Columns { get; set; }

        public IEnumerable<dynamic> Data { get; set; }

        public long Count => Data.Count();

        public ExecutionResponseResult()
        {
            Columns = new List<ExecutionResponseResultColumn>();
        }
    }

    public class ExecutionResponseResultColumn
    {
        public string Name { get; set; }

        public string Label { get; set; }
    }
}
