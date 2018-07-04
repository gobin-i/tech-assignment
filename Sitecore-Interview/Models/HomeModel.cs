using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;

namespace Sitecore_Interview.Models
{
    public class HomeModel
    {

        //Private 
        public DataTable seoAnalyzer { get; set; }

        [Display(Name ="Content/Single URL",Description = "")]
        [Required(AllowEmptyStrings =false)]
        public string textContent { get; set; }

        [Display(Name = "Filter Words (separated by commas)", Description = "(e.g. ‘or’, ‘and’, ‘a’, ‘the’ etc),")]
        public string filterStopWords { get; set; }

        [Display(Name = "Do you want to count number occurance of words in keyword meta tag?", Description = "")]
        public bool isCountOfMetaKeywordsRequired { get; set; }

        [Display(Name = "Do you want to count number of occurance of external links in scraped URL", Description = "")]
        public bool isCountOfExternalLinksRequired { get; set; }

        [Display(Name = "Do you want to scrape URL?", Description = "")]
        public bool isUrlToScrape { get; set; }

        public HomeModel() {
            //Init DataTable
            initDataTable();
        }
        public HomeModel(string textContent,string filterStopWords,bool isCountOfMetaKeywordsRequired,bool isCountOfExternalLinksRequired) {

            this.textContent = textContent;
            this.filterStopWords = filterStopWords;
            this.isCountOfMetaKeywordsRequired = isCountOfMetaKeywordsRequired;
            this.isCountOfExternalLinksRequired =isCountOfExternalLinksRequired;

        }

        public string getStringFromUrl(string url)
        {
            WebClient client = new WebClient();
            try
            {
                string response = client.DownloadString(url);
                string contentType = client.ResponseHeaders["Content-Type"];
                if(contentType.Contains("text/html"))
                {
                    return response;
                }

                return null; //For other than HTML type of content
            }
            catch (WebException e)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Unable to read content of "+url+" due to error below:"+e.Message, EventLogEntryType.Error);
                }
            }

            return null;
        }

        public bool isValidUrl(string url) {

            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

        private void  initDataTable() {

            //Init DataTable - to store data
            DataColumn columnWord = new DataColumn("word", typeof(string));
            DataColumn columnTotal = new DataColumn("total", typeof(int));
            DataColumn columnCategory = new DataColumn("category", typeof(string));
            seoAnalyzer.Columns.Add(columnWord);
            seoAnalyzer.Columns.Add(columnTotal);
            seoAnalyzer.Columns.Add(columnCategory);

        }

        public DataTable getSeoAnalyzer() {

            return seoAnalyzer;
        }

        public bool upsertWord(string word, string category,int count)
        {
            var result = from row in seoAnalyzer.AsEnumerable() where row.Field<string>("word") == word && row.Field<string>("category") == category select row;
            

            if (result != null)
            {
                if (result.Count() > 0)
                {
                    //Exist and Update
                   
                    result.FirstOrDefault()["total"] = (int)result.FirstOrDefault()["total"] + count;
                    result.FirstOrDefault().AcceptChanges();
                }
                else {
                    //Add New
                    seoAnalyzer.Rows.Add(word, count, category);
                }



            }
            else {
                seoAnalyzer.Rows.Add(word, count, category);
            }
            return false;
        }



    }
}