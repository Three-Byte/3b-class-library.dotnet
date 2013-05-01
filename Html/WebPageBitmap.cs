using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;

namespace ThreeByte.Html {

    public static class WebPageThumbnail {

        public static Bitmap GetWebThumbnail(string url, int width, int height) {
            Bitmap thumbnail = null;
            WebPageBitmap webBitmap = new WebPageBitmap(url, width, height, false, 10000);
            if(webBitmap.IsOk) {
                webBitmap.Fetch();
                thumbnail = webBitmap.GetBitmap(width, height);                
            }

            return thumbnail;
        }
    }


    /// <summary>
    /// Thanks for the solution to the "sometimes not painting sites to Piers Lawson
    /// Who performed some extensive research regarding the origianl implementation.
    /// You can find his codeproject profile here:
    /// http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=39324
    /// </summary>
    [InterfaceType(1)]
    [Guid("3050F669-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLElementRender2 {
        void DrawToDC(IntPtr hdc);
        void SetDocumentPrinter(string bstrPrinterName, ref _RemotableHandle hdc);
    }

    /// <summary>
    /// Code by Adam Najmanowicz
    /// http://www.codeproject.com/script/Membership/Profiles.aspx?mid=923432
    /// http://blog.najmanowicz.com/
    /// 
    /// Some improvements suggested by Frank Herget
    /// http://www.artviper.net/
    /// </summary>
    class WebPageBitmap {
        private WebBrowser webBrowser;
        private string url;
        private int width;
        private int height;
        private bool isOk;
        private string errorMessage;

        public string ErrorMessage {
            get { return errorMessage; }
        }

        public bool IsOk {
            get { return isOk; }
            set { isOk = value; }
        }

        public WebPageBitmap(string url, int width, int height, bool scrollBarsEnabled, int wait) {
            this.width = width;
            this.height = height;


            this.url =
                url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ?
                url : this.url = "http://" + url;

            try
                // needed as the script throws an exeception if the host is not found
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.url);
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:20.0) Gecko/20100101 Firefox/20.0";
                req.Referer = "http://www.cognifide.com";
                req.ContentType = "text/html";
                req.Accept = "*/*";
                req.KeepAlive = false;

                using(HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                    string x = resp.StatusDescription;
                }

            } catch(Exception ex) {
                errorMessage = ex.Message;
                isOk = false;
                return;
            }
            isOk = true;                                                      // public, to check in program.cs if the domain is found, so the image can be saved

            webBrowser = new WebBrowser();
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
            webBrowser.Size = new Size(width, height);
            webBrowser.ScrollBarsEnabled = false;
        }

        /// <summary>
        /// Fetches the image 
        /// </summary>
        /// <returns>true is the operation ended with a success</returns>
        public bool Fetch() {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.AllowAutoRedirect = true;
            req.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; .NET CLR 3.0.04506; .NET CLR 3.5.21022; .NET CLR 1.0.3705; .NET CLR 1.1.4322)";
            req.Referer = "http://www.cognifide.com";
            req.ContentType = "text/html";
            req.AllowWriteStreamBuffering = true;
            req.AutomaticDecompression = DecompressionMethods.GZip;
            req.Method = "GET";
            req.Proxy = null;
            req.ReadWriteTimeout = 20;

            HttpStatusCode status;
            using(HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                status = resp.StatusCode;
            }

            if(status == HttpStatusCode.OK || status == HttpStatusCode.Moved) {
                webBrowser.Navigate(url);
                while(webBrowser.ReadyState != WebBrowserReadyState.Complete) {
                    Application.DoEvents();

                }
                return true;
            } else {
                return false;
            }
        }

        private void documentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs e) {
            ((WebBrowser)sender).Document.Window.Error +=
                new HtmlElementErrorEventHandler(SuppressScriptErrorsHandler);
        }

        public void SuppressScriptErrorsHandler(object sender, HtmlElementErrorEventArgs e) {
            e.Handled = true;
            MessageBox.Show("Error!");
        }

        internal Bitmap GetBitmap(int thumbwidth, int thumbheight) {
            IHTMLDocument2 rawDoc = (IHTMLDocument2)webBrowser.Document.DomDocument;
            IHTMLElement rawBody = rawDoc.body;
            IHTMLElementRender2 render = (IHTMLElementRender2)rawBody;

            Bitmap bitmap = new Bitmap(width, height);
            Rectangle bitmapRect = new Rectangle(0, 0, width, height);

            // Interesting thing that despite using the renderer later 
            // this following line is still necessary or 
            // the background may not be painted on some websites.
            webBrowser.DrawToBitmap(bitmap, bitmapRect);

            using(Graphics graphics = Graphics.FromImage(bitmap)) {
                IntPtr graphicshdc = graphics.GetHdc();
                render.DrawToDC(graphicshdc);

                graphics.ReleaseHdc(graphicshdc);
                graphics.Dispose();

                if(thumbheight == height && thumbwidth == width) {
                    return bitmap;
                } else {
                    Bitmap thumbnail = new Bitmap(thumbwidth, thumbheight);
                    using(Graphics gfx = Graphics.FromImage(thumbnail)) {
                        // high quality image sizing
                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;                                                                       // make it look pretty 
                        gfx.DrawImage(bitmap, new Rectangle(0, 0, thumbwidth, thumbheight), bitmapRect, GraphicsUnit.Pixel);
                    }
                    bitmap.Dispose();
                    return thumbnail;
                }
            }
        }
    }
}
