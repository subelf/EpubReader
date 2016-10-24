using System;
using System.Linq;
using Foundation;
using UIKit;
using VersFx.Formats.Text.Epub;

namespace EpubReaderDemo.iOS
{
    public partial class InitViewController : UIViewController
    {
        public InitViewController() : base("InitViewController", null)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var path = NSBundle.MainBundle.PathForResource("test", "epub");
            var stream = NSData.FromFile(path).AsStream();

            var epub = EpubReader.OpenBook(stream);
            var s = epub.Schema;


            var page = epub.Content.Css.Values.ToList()[0];//();

            var webview = new UIWebView(View.Bounds);
            View.AddSubview(webview);
            webview.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;

            var ss = NSString.FromData(NSData.FromStream(page.GetContent()), NSStringEncoding.UTF8);

            webview.LoadHtmlString(ss, null);


            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}