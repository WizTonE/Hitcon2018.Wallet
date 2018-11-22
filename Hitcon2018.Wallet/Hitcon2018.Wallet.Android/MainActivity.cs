using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;
using Android;
using System.Linq;

namespace Hitcon2018.Wallet.Droid
{
    [Activity(Label = "Hitcon2018.Wallet", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            UserDialogs.Init(() => this);
            base.OnCreate(bundle);
            LoadApplication(new App());

            if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
                RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation }, 1);
            if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted)
                RequestPermissions(new string[] { Manifest.Permission.Camera }, 1);
            if (CheckSelfPermission(Manifest.Permission.Bluetooth) != Permission.Granted)
                RequestPermissions(new string[] { Manifest.Permission.Bluetooth }, 1);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //try
            //{
            //ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //    this.RequestPermissions(new[]
            //    {
            //        Manifest.Permission.AccessCoarseLocation,
            //        Manifest.Permission.BluetoothPrivileged
            //    }, 0);
            //}
            //catch (Exception ex)
            //{

            //}
            if (grantResults.Any(p => p != Permission.Granted))
            {
                new AlertDialog.Builder(this).SetTitle("Error").SetMessage("This app need CoarseLocation and Bluetooth permission").SetPositiveButton("OK", (s, e) => Finish());
            }
        }


    }
}

