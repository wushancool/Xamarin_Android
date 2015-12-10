using SQLite;
using System;

namespace HaierAppTest
{
    public class BarCodeInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(18)]
        public string BarCode { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUser { get; set; }
        public bool IsSynchronous { get; set; }
        public bool IsSave { get; set; }
    }
}