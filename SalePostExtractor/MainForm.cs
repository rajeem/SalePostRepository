using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using mshtml;

namespace SalePostScraper
{
    public partial class MainForm : Form
    {
        List<string> list;

        public MainForm()
        {
            InitializeComponent();
            list = new List<string>();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            var defaultLoc = "baguio city";

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(browser.DocumentText);
            var salePosts = doc.DocumentNode.SelectNodes("//*[contains(@class,'_5pcb')]//a");
            foreach (var item in salePosts)
            {
                var url = item.GetAttributeValue("href", "");
                list.Add(url);
            }
            foreach (var item in list)
            {
                browser.Navigate("https://www.facebook.com/" + item);

                //todo: optimize(not a good solution)
                while (browser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                }
                doc.LoadHtml(browser.DocumentText);
                var scope = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'userContentWrapper')]");

                var title = scope.SelectNodes("./div[1]/div[5]/div[1]/div[1]/div[1]/div[1]")[0].InnerText;
                var price = scope.SelectNodes("./div[1]/div[5]/div[1]/div[1]/div[1]/div[2]/div[1]")[0].InnerText;
                var xpathLoc = "./div[1]/div[5]/div[1]/div[1]/div[1]/div[2]/div[2]";
                var location = scope.SelectNodes(xpathLoc) != null ? scope.SelectNodes(xpathLoc)[0].InnerText : defaultLoc;
                var body = scope.SelectNodes("./div[1]/div[5]/div[1]/div[1]/div[1]/div[3]")[0].InnerText;

                var userRaw = scope.SelectNodes("./div[1]/div[3]//a")[1].Attributes["href"].Value;
                var userId = Regex.Match(userRaw, @"\?id=([^&]*)").ToString().Replace("?id=", "");

                var images = GetImages();

                MessageBox.Show("USER: " + userId + "\nTITLE: " + title + "\nPRICE: " + price + "\nLOCATION: " + location + "\nBODY: " + body);
            }
        }

        private IEnumerable<Bitmap> GetImages()
        {
            IHTMLDocument2 doc2 = (IHTMLDocument2)browser.Document.DomDocument;
            IHTMLControlRange imgRange = (IHTMLControlRange)((HTMLBody)doc2.body).createControlRange();

            var list = new List<Bitmap>();
            foreach (IHTMLImgElement img in doc2.images)
            {
                if (img.src.StartsWith("https://fbcdn-photos"))
                {
                    imgRange.add((IHTMLControlElement)img);
                    imgRange.execCommand("Copy", false, null);
                    using (Bitmap bmp = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap))
                    {
                        list.Add(bmp);
                    }
                }
            }
            return list;
        }

        private void btnNavigate_Click(object sender, EventArgs e)
        {
            browser.Navigate(txtUrl.Text);
        }

        private void btnViewSource_Click(object sender, EventArgs e)
        {
            rtbSource.Text = browser.DocumentText;
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            txtUrl.Text = browser.Url.ToString();
        }
    }
}
