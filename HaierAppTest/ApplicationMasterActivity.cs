using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ZXing.Mobile;
using SQLite;
using Android.Views.Animations;

namespace HaierAppTest
{
    [Activity(Label = "海尔ACG")]
    public class ApplicationMasterActivity : Activity
    {
        private MobileBarcodeScanner scanner;
        private ListView lv;
        private ArrayAdapter<string> _adapter;
        private List<BarCodeInfo> barCodeList;
        private List<string> barCodes;
        private SQLiteConnection sqliConn;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.ApplicationMaster);
            barCodes = new List<string>();

            SQLiteHelper sql = new SQLiteHelper();
            sqliConn = sql.SQLConn;

            scanner = new MobileBarcodeScanner();
            Button barcodeBtn = this.FindViewById<Button>(Resource.Id.btnBarcode);
            Button btnback = this.FindViewById<Button>(Resource.Id.btnback);
            Button btnsave = this.FindViewById<Button>(Resource.Id.btnsave);
            Button btntempsave = this.FindViewById<Button>(Resource.Id.btntempsave);
            
            lv = this.FindViewById<ListView>(Resource.Id.listViewBarCode);
            InitList();

            //MobileBarcodeScanner scanner;
            barcodeBtn.Click += async delegate
            {
                #region 自定义窗口1
                //Tell our scanner to use the default overlay
                scanner.UseCustomOverlay = false;
                //We can customize the top and bottom text of the default overlay
                scanner.TopText = "相机对准条维码，距离大概6厘米左右";
                scanner.BottomText = "请等待条码自动扫描";
                //Start scanning
                var result = await scanner.Scan();

                HandleScanResult(result);
                #endregion
                
            };

            Button flashButton;
            View zxingOverlay;
            TextView textLine;
            Button btnFlashBarCode = this.FindViewById<Button>(Resource.Id.btnFlashBarcode);
            btnFlashBarCode.Click += async delegate 
            {
                #region 自定义窗口2（包含开启闪光灯）
                //Tell our scanner we want to use a custom overlay instead of the default
                scanner.UseCustomOverlay = true;

                //Inflate our custom overlay from a resource layout
                zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

                //Find the button from our resource layout and wire up the click event
                flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
                //添加动画
                textLine = zxingOverlay.FindViewById<TextView>(Resource.Id.textView2);
                var anim = AnimationUtils.LoadAnimation(this, Resource.Animation.in_from_bottom);
                anim.FillAfter = true;
                textLine.StartAnimation(anim);
                
                flashButton.Click += delegate
                {
                    if (!scanner.IsTorchOn)
                    {
                        scanner.Torch(true);
                        flashButton.Text = "关闭闪光灯";
                    }
                    else
                    {
                        scanner.Torch(false);
                        flashButton.Text = "开启闪光灯";
                    }
                };

                //Set our custom overlay
                scanner.CustomOverlay = zxingOverlay;

                //Start scanning!
                var result = await scanner.Scan();

                HandleScanResult(result);
                #endregion
            };
            // Create your application here
            btnback.Click += Btnback_Click;
            btntempsave.Click += Btntempsave_Click;
            btnsave.Click += Btnsave_Click;
        }

        private void InitList()
        {
            barCodeList = sqliConn.Table<BarCodeInfo>().ToList();
            
            if (barCodeList != null && barCodeList.Count > 0)
            {
                foreach (var item in barCodeList)
                {
                    barCodes.Add(item.BarCode);
                }
                _adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, barCodes.ToArray<string>());
                lv.Adapter = _adapter;
            }
        }

        private void Btntempsave_Click(object sender, EventArgs e)
        {
            sqliConn.DeleteAll<BarCodeInfo>();
            List<BarCodeInfo> saveListObj = barCodeList.Where(m => m.IsSave == false).ToList();
            int res= sqliConn.InsertAll(saveListObj);
            this.RunOnUiThread(() => Toast.MakeText(this, "成功临时保存【" + res.ToString() + "】条", ToastLength.Short).Show());
        }

        private void Btnsave_Click(object sender, EventArgs e)
        {
            
        }

        private void Btnback_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        void HandleScanResult(ZXing.Result result)
        {
            string msg = "";
            
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                if (barCodes.Contains(result.Text))
                {
                    msg = "此条码【 " + result.Text + "】已经存在,不能重复扫描";
                }
                else
                {
                    msg = "条码: " + result.Text;
                    barCodeList.Add(new BarCodeInfo
                    {
                        BarCode = result.Text,
                        CreateDate = DateTime.Now,
                        IsSave = false,
                        IsSynchronous = false
                    });
                    barCodes.Add(result.Text);
                    _adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, barCodes.ToArray());


                    lv.Adapter = _adapter;
                }
            }
            else
            { 
                msg = "扫描取消";
            }

            this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
        }

        private DateTime? lastBackKeyDownTime;
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                if (!lastBackKeyDownTime.HasValue || DateTime.Now - lastBackKeyDownTime.Value > new TimeSpan(0, 0, 2))
                {
                    Toast.MakeText(this.ApplicationContext, "再按一次退出程序", ToastLength.Short).Show();

                    lastBackKeyDownTime = DateTime.Now;
                }
                else
                {
                    Intent exitIntent = new Intent(Intent.ActionMain);
                    exitIntent.AddCategory(Intent.CategoryHome);
                    exitIntent.SetFlags(ActivityFlags.NewTask);
                    StartActivity(exitIntent);
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                }
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}