using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using System.Net;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.IO;

namespace NardyHondaScraper
{
    public class NardyHondaExtraction
    {
        public static List<string> _siteMap = new List<string>()
        {
            "https://www.nardyhonda.com/sitemap.xml"
        };
        public static List<VehicleUrls> _vehicleUrls = new List<VehicleUrls>();
        public class VehicleUrls
        {
            public string VehicleURL { get; set; }
        }

        public static List<VehicleInfo> _vehicleList = new List<VehicleInfo>();
        public class VehicleInfo
        {
            public string YearAndMake { get; set; } = string.Empty;
            public string NameAndVin { get; set; } = string.Empty;
            public string Mpg { get; set; } = string.Empty;
            public string Price { get; set; } = string.Empty;
        }

        public class DealerInfo
        {
            public string DealerName { get; set; } = string.Empty;
            public string DealerAddress { get; set; } = string.Empty;
            public string DealerPhoneNumber { get; set; } = string.Empty;
        }

        public static void ExtractVehicleUrls()
        {
            DealerInfo dealerInfo = new DealerInfo();

            /*Create a new instance of the System.Net Webclient*/
            WebClient wc = new WebClient();
            /*Set the Encodeing on the Web Client*/
            wc.Encoding = System.Text.Encoding.UTF8;

            foreach (var sitemapUrls in _siteMap)
            {
                /* Download the document as a string*/
                string siteMapString = wc.DownloadString(sitemapUrls);
                /*Create a new xml document*/
                XmlDocument urldoc = new XmlDocument();
                /*Load the downloaded string as XML*/
                urldoc.LoadXml(siteMapString);
                /*Create an list of XML nodes from the url nodes in the sitemap*/
                XmlNodeList xmlSitemapList = urldoc.GetElementsByTagName("url");
                /*Loops through the node list and prints the values of each node*/
                foreach (XmlNode node in xmlSitemapList)
                {
                    if (node["loc"].InnerText.Contains("https://www.nardyhonda.com/new/Honda/"))
                    {
                        _vehicleUrls.Add(new VehicleUrls
                        {
                            VehicleURL = node["loc"].InnerText
                        });
                    }
                }

                ExtractDealerInfo(wc, dealerInfo);
                ExtractVehicleData(wc, _vehicleUrls);
                WriteExtracteDataToCSV(_vehicleList, dealerInfo);
            }
        }

        public static void ExtractDealerInfo(WebClient wc, DealerInfo dealerInfo)
        {
            try
            {
                /* Download the document as a string*/
                string htmlString = wc.DownloadString("https://www.nardyhonda.com/contact.htm");
                /*Create a new xml document*/
                HtmlDocument urldoc = new HtmlDocument();
                /*Load the downloaded string as XML*/
                urldoc.LoadHtml(htmlString);

                dealerInfo.DealerName = urldoc.DocumentNode.SelectSingleNode("//address/p/strong").InnerText;
                dealerInfo.DealerAddress = urldoc.DocumentNode.SelectSingleNode("//address/p/span").InnerText;
                dealerInfo.DealerPhoneNumber = urldoc.DocumentNode.SelectSingleNode("//span/span[@data-phone-ref='SALES']").InnerText;
            }
            catch (Exception e)
            {

            }

        }


        public static void ExtractVehicleData(WebClient wc, List<VehicleUrls> vehicleUrls)
        {
            try
            {
                foreach (var vehicleUrl in vehicleUrls)
                {
                    /* Download the document as a string*/
                    string htmlString = wc.DownloadString(vehicleUrl.VehicleURL);
                    /*Create a new xml document*/
                    HtmlDocument urldoc = new HtmlDocument();
                    /*Load the downloaded string as XML*/
                    urldoc.LoadHtml(htmlString);

                    string yearAndMake = urldoc.DocumentNode.SelectSingleNode("//h1/span[@class='d-block h4 text-muted font-weight-normal BLANK']").InnerText;
                    string nameAndVin = urldoc.DocumentNode.SelectSingleNode("//span[@class='BLANK']").InnerText;
                    string mpg = urldoc.DocumentNode.SelectNodes("//dd/span").First().InnerText;
                    string price = urldoc.DocumentNode.SelectSingleNode("//span[@class='price-value']").InnerText.Replace(",", string.Empty);

                    AddToVehicleList(_vehicleList, yearAndMake, nameAndVin, mpg, price);
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {

            }

        }

        public static void AddToVehicleList(List<VehicleInfo> vehicleList, string yearAndMake, string nameAndVin, string mpg, string price)
        {
            vehicleList.Add(new VehicleInfo
            {
                YearAndMake = yearAndMake,
                NameAndVin = nameAndVin,
                Mpg = mpg,
                Price = price
            });
        }

        public static void WriteExtracteDataToCSV(List<VehicleInfo> extractedData, DealerInfo dealerInfo)
        {
            var csvBuilder = new StringBuilder();

            csvBuilder.AppendLine(string.Format("{0}", dealerInfo.DealerName));
            csvBuilder.AppendLine(string.Format("{0}", dealerInfo.DealerAddress));
            csvBuilder.AppendLine(string.Format("{0}", dealerInfo.DealerPhoneNumber));
            csvBuilder.AppendLine(string.Format("{0}", string.Empty));

            foreach (var data in extractedData)
            {
                csvBuilder.AppendLine(string.Format("{0},{1},{2},{3}", data.YearAndMake, data.NameAndVin, data.Mpg, data.Price));
            }

            File.WriteAllText("C:\\Users\\Misael Lopez\\Desktop\\Text CSV Files\\csvfile.csv", csvBuilder.ToString());
        }
    }
}
