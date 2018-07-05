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

        /// <value>Get/Set the value of words that has been analyzed</value>
        public DataTable seoAnalyzer { get; set; }

        /// <value>Get/Set the value of input (text/url) </value>
        [Display(Name ="Content/Single URL",Description = "")]
        [Required(AllowEmptyStrings =false)]
        public string textContent { get; set; }

        /// <value>Get/Set the value of filter words (separated by commas)</value>
        [Display(Name = "Filter Words (separated by commas)", Description = "(e.g. ‘or’, ‘and’, ‘a’, ‘the’ etc),")]
        [Required(AllowEmptyStrings = false)]
        public string filterStopWords { get; set; }

        /// <value>Get/Set the flag if the SEO analyze need to process the keywords meta tag</value>
        [Display(Name = "Do you want to count number occurance of words in keyword meta tag?", Description = "")]
        public bool isCountOfMetaKeywordsRequired { get; set; }

        /// <value>Get/Set the flag if the SEO analyze need to process the external links found in page (inclusive of relative path)</value>
        [Display(Name = "Do you want to count number of occurance of external links in scraped URL", Description = "")]
        public bool isCountOfExternalLinksRequired { get; set; }

        /// <summary>
        /// Contructor with DataTable initialization to prepare in-memory data storage
        /// </summary>
        public HomeModel() {
            //Init DataTable
            initDataTable();
        }

        /// <summary>
        /// Contructor with parameters with DataTable initialization to prepare in-memory data storage
        /// </summary>
        public HomeModel(string textContent,string filterStopWords,bool isCountOfMetaKeywordsRequired,bool isCountOfExternalLinksRequired) {

            this.textContent = textContent;
            this.filterStopWords = filterStopWords;
            this.isCountOfMetaKeywordsRequired = isCountOfMetaKeywordsRequired;
            this.isCountOfExternalLinksRequired =isCountOfExternalLinksRequired;
            initDataTable();
        }

        /// <summary>
        /// Contructor with parameters with DataTable initialization to prepare in-memory data storage
        /// </summary>
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

        /// <summary>
        /// Initialize DataTable to contain three columns namely Word, Total, and Category
        /// </summary>
        private void  initDataTable() {

            //Init DataTable - to store data
            seoAnalyzer = new DataTable();
            DataColumn columnWord = new DataColumn("word", typeof(string));
            DataColumn columnTotal = new DataColumn("total", typeof(int));
            DataColumn columnCategory = new DataColumn("category", typeof(string));
            seoAnalyzer.Columns.Add(columnWord);
            seoAnalyzer.Columns.Add(columnTotal);
            seoAnalyzer.Columns.Add(columnCategory);

        }

        /// <summary>
        /// This method will Insert if the word matched to the category doesn't exist and update the total count if it does match
        /// </summary>
        public bool upsertWord(string word, string category,int count)
        {
            if (seoAnalyzer != null)
            {
                var result = from row in seoAnalyzer.AsEnumerable() where row.Field<string>("word") == (word != null ? word : "N/A") && row.Field<string>("category") == (category != null ? category : "N/A") select row;

                if (result != null)
                {
                    if (result.Count() > 0)
                    {
                        //Exist and Update
                        result.FirstOrDefault()["total"] = (int)result.FirstOrDefault()["total"] + count;
                        result.FirstOrDefault().AcceptChanges();
                        return true;
                    }
                    else
                    {
                        //Add New
                        seoAnalyzer.Rows.Add(word, count, category);
                        return true;
                    }
                }


            }

            return false;
        }



    }
}