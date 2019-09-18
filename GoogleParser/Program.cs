using System;
using System.IO;
using System.Net;
using System.Text;
using AngleSharp.Html.Parser;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GoogleParser
{
    class Program
    {
        private static readonly string firstPageUrl = "https://www.google.com.ua/search?q=it-enterprise&hl=ru&source=lnt&tbs=qdr:d&biw=1920&bih=969";
        private static readonly string secondPageUrl = "https://www.google.com.ua/search?q=it-enterprise&hl=ru&source=lnt&tbs=qdr:d&start=10&biw=1920&bih=969";
        private static readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Result.pdf";
        private static readonly string FONT_DIR = "c:\\windows\\fonts\\arial.ttf";
        private static readonly WebClient wc = new WebClient();
        private static readonly HtmlParser parcer = new HtmlParser();

        static void Main(string[] args)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                iTextSharp.text.Document PDFdocument = new iTextSharp.text.Document();
                var writer = PdfWriter.GetInstance(PDFdocument, fs);
                Paragraph paragraph = GetParagraph();
                PDFdocument.Open();

                GenerateAndInsert(firstPageUrl, paragraph);
                GenerateAndInsert(secondPageUrl, paragraph);
                if (paragraph.Count == 0)
                {
                    paragraph.Add("no matches found...");
                }
                PDFdocument.Add(paragraph);
                PDFdocument.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();

            }
        }

        private static string GetTrancatedUrl(AngleSharp.Dom.IElement container)
        {
            string url = container.QuerySelector(".r > a:first-child").GetAttribute("href");
            url = url.Substring(7, url.Length - 9);
            return url.Substring(0, url.IndexOf("&"));
        }

        private static bool Check(string textFromContainer)
        {
            return textFromContainer.ToLower().Contains("it-enterprise");
        }

        private static Paragraph GetParagraph()
        {
            BaseFont baseFont = BaseFont.CreateFont(FONT_DIR, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, Font.DEFAULTSIZE, Font.NORMAL);
            Paragraph paragraph = new Paragraph("", font);
            return paragraph;
        }

        private static bool EvaluateUrl(AngleSharp.Dom.IElement container)
        {
            var currentUrl = container.QuerySelector(".r > a:first-child").GetAttribute("href");
            return currentUrl.Contains("youtube.com") || currentUrl.Contains("search?biw")
                || currentUrl.Contains("rabota.ua") || currentUrl.Contains("it.ua") || currentUrl.Contains("it-enterprise.net")
                || currentUrl.Contains("work.ua") || currentUrl.Contains("it-enterprise.com");
        }

        private static void GenerateAndInsert(string url, Paragraph paragraph)
        {
            var html = Encoding.GetEncoding(1251).GetString(wc.DownloadData(url));
            var document = parcer.ParseDocument(html);
            var containers = document.GetElementsByClassName("g");

            foreach (var currentCointainer in containers)
            {
                if (!EvaluateUrl(currentCointainer))
                {
                    var text = currentCointainer.QuerySelectorAll("[class='st']")[0].TextContent;
                    if (Check(text))
                    {
                        paragraph.Add(currentCointainer.GetElementsByTagName("h3")[0].TextContent);
                        paragraph.Add("\n");
                        paragraph.Add(new Anchor(GetTrancatedUrl(currentCointainer), FontFactory.GetFont(FontFactory.HELVETICA, 14, new BaseColor(0, 0, 255))));
                        paragraph.Add("\n");
                        paragraph.Add(text);
                        paragraph.Add("\n");
                        paragraph.Add("--------------------------------------------------");
                        paragraph.Add("\n\n");
                    }
                }
            }
        }
    }
}
