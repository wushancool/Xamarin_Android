using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using HaierAppTest.com.boldseas.acgserver;

namespace HaierAppTest
{
    public class SQLiteHelper
    {
        private SQLiteConnection conn;
        public SQLiteHelper()
        {
            try
            {
                string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                conn = new SQLiteConnection(System.IO.Path.Combine(folder, "HaierACG.db"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void onCreate()
        {
            try
            {
                conn.CreateTable<BarCodeInfo>();
                conn.CreateTable<UserInfo>();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public SQLiteConnection SQLConn
        {
            get { return conn; }
        }
    }
}