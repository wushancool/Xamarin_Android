using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using HaierAppTest.com.boldseas.acgserver;
using SQLite;

namespace HaierAppTest
{
    [Activity(Label = "海尔ACG", MainLauncher = true, Icon = "@drawable/haieracg")]
    public class MainActivity : Activity
    {
        #region 私有对象
        private string msg;
        private SQLiteConnection sqliConn;
        #endregion

        #region OnCreate
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            SQLiteHelper sql = new SQLiteHelper();
            sql.onCreate();
            sqliConn = sql.SQLConn;

            HaierAppTest.com.boldseas.acgserver.AccountService account = new com.boldseas.acgserver.AccountService();
            //account.UserLogin()

            Button button = FindViewById<Button>(Resource.Id.btn_login);
            Button btnExit = FindViewById<Button>(Resource.Id.btn_exit);

            EditText txtLoginName = FindViewById<EditText>(Resource.Id.txt_login_name);
            EditText txtLoginPwd = FindViewById<EditText>(Resource.Id.txt_login_pwd);
            TextView txtMsg = FindViewById<TextView>(Resource.Id.form_title);
            button.Click += delegate {
                if(string.IsNullOrEmpty(txtLoginName.Text))
                { 
                    AlertDialog alert = new AlertDialog.Builder(this)
                        .SetTitle("登陆验证")
                        .SetMessage("请输入用户名.")
                        .SetPositiveButton("确定", delegate { })
                        .Create();
                    alert.Show();
                    return;
                }

                if (string.IsNullOrEmpty(txtLoginPwd.Text))
                {
                    msg = "请输入密码";
                    this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
                }
                
                SYS_User loginUser = new SYS_User();
                loginUser.LoginName= txtLoginName.Text;
                loginUser.PassWord= txtLoginPwd.Text;
                UserFeedbackStatus loginStatus;
                bool loginRes;
                SYS_User resUser = new SYS_User();
                account.UserLogin(loginUser, out loginStatus, out loginRes, out resUser);

                if (loginStatus==UserFeedbackStatus.Success)
                {
                    txtMsg.Text = "登陆成功！";
                    sqliConn.DeleteAll<UserInfo>();
                    //保存登陆用户
                    sqliConn.Insert(new UserInfo
                    {
                        UserName = resUser.Name_CN,
                        LoginDateTime = DateTime.Now,
                        Id = resUser.UserID
                    });

                    //显示系统主页
                    StartActivity(typeof(ApplicationMasterActivity));
                }
            };


            btnExit.Click += BtnExit_Click;
            // Get our button from the layout resource,
            // and attach an event to it
            //Button button = FindViewById<Button>(Resource.Id.MyButton);

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
        }
        #endregion

        #region 退出事件
        private void BtnExit_Click(object sender, EventArgs e)
        {
            Intent exitIntent = new Intent(Intent.ActionMain);
            exitIntent.AddCategory(Intent.CategoryHome);
            exitIntent.SetFlags(ActivityFlags.NewTask);
            StartActivity(exitIntent);
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());

            //另外，this.Finish();
        }
        #endregion
    }
}

