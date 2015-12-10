using System;
using System;
using SQLite;

namespace HaierAppTest
{
    public class UserInfo
    {
        [PrimaryKey]
        public int Id { get; set; }
        
        public string UserName { get; set; }
        public DateTime LoginDateTime { get; set; }
    }
}