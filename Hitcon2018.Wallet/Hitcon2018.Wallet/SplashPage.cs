using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Hitcon2018.Wallet
{
	public class SplashPage : ContentPage
	{
        Image splashImage;
		public SplashPage ()
		{
            NavigationPage.SetHasNavigationBar(this, false);

            var sub = new AbsoluteLayout();
            splashImage = new Image
            {
                Source = "Hiconv7_01.png",
                Aspect = Aspect.AspectFill
            };

            AbsoluteLayout.SetLayoutFlags(splashImage,
                AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(splashImage,
                new Rectangle(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            sub.Children.Add(splashImage);

            this.BackgroundColor = Color.FromHex("#429de3");
            this.Content = sub;
		}

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await splashImage.ScaleTo(1, 2000);
            await splashImage.ScaleTo(0.9, 1500, Easing.Linear);
            await splashImage.ScaleTo(150, 1200, Easing.Linear);
            Application.Current.MainPage = new NavigationPage(new WalletPage());
        }
    }
}