namespace NewGarasAPI.Models.Admin
{
    public class GetDBTablesColumnsData
    {
        public string TableName { get; set; }
        public List<string> ColumnsList { get; set; }
    }
}