using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using Sitecore_Interview.Models;
namespace Sitecore_Interview.Controllers
{
    /// <summary>
    /// Home Controller Class will handle the HTTP request that triggered from user's browser/action based on HTTP VERB and routes
    /// </summary>
    public class HomeController : Controller
    {

        /// <returns>
        /// Will returns default view when user browser to homepage - Only accessible via HTTP GET verb
        /// </returns>
        [HttpGet]
        public ActionResult Index()
        {
            HomeModel homeModel = new HomeModel();
            return View(homeModel);
        }

        /// <summary>
        /// Return view after postback - only accessible via HTTP POST verb
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HomeModel homeModel)
        {
           
            if (ModelState.IsValid) //To check if the submitted model is valid in terms of field requirements
            {
                //Check if the input is a valid URL
                Uri uriResult;
                bool isValidUrl = Uri.TryCreate(homeModel.textContent, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                //If the input provided is valid URL, the content input will be the URL
                if (isValidUrl) 
                {
                    homeModel.textContent = homeModel.getStringFromUrl(homeModel.textContent); //For valid url as content input, we will scrape the content
                }

                //Check if the content input is empty
                if (!string.IsNullOrEmpty(homeModel.textContent))
                {
                    //Check the occurrence of stopword in the content input
                    foreach (string stopWord in homeModel.filterStopWords.Split(','))
                    {
                        MatchCollection matchColl = Regex.Matches(homeModel.textContent, @"\b" + stopWord + @"\b", RegexOptions.IgnoreCase);

                        //Update to datatable
                        homeModel.upsertWord((stopWord!=null?stopWord:"N/A"), (isValidUrl ? "WEBPAGE" : "TEXT"), (matchColl!=null?matchColl.Count:0));
                    }

                    //If the content input is a valid URL, then scrape the url
                    if (isValidUrl)
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(homeModel.textContent);

                        //Extract meta tags from html string
                        if (homeModel.isCountOfMetaKeywordsRequired)
                        {

                            var node = doc.DocumentNode.SelectSingleNode("//head/meta[@name=\"keywords\"]");
                            if (node != null)
                            {

                                string keywords = node.GetAttributeValue("content", null);
                                if (!string.IsNullOrEmpty(keywords))
                                {

                                    foreach (string word in keywords.Split(','))
                                    {
                                        //Getting regular expression matching for the word found in keywords
                                        MatchCollection matchColl = Regex.Matches(homeModel.textContent, @"\b" + word + @"\b", RegexOptions.IgnoreCase);

                                        //Update to datatable
                                        homeModel.upsertWord((word!=null?word:"N/A"), "META-KEYWORD", (matchColl!=null?matchColl.Count - 1:0));
                                    }
                                }
                                else
                                {

                                    //display message that there wasn't any value associated with the meta tag
                                    ViewBag.Message = "There's no keyword meta tags value provided.";
                                }

                            }
                            else
                            {

                                //display message that the meta tag keyword is not found
                                ViewBag.Message = "There's no keyword meta tags found in provided URL.";
                            }

                        }
                        //End of getting meta tag[keywords] value

                        //Extract external link from the page
                        if (homeModel.isCountOfExternalLinksRequired)
                        {

                            var nodes = doc.DocumentNode.SelectNodes("//a");

                            foreach (HtmlNode node in nodes)
                            {
                                string link = node.GetAttributeValue("href", null);
                                if (!string.IsNullOrEmpty(link))
                                {
                                    if (link[0] != '#')
                                    {
                                        Uri uriTmp;
                                        link = (link[0] == '/' ? uriResult.DnsSafeHost + link : link);
                                        if (Uri.TryCreate(link, UriKind.Absolute, out uriTmp))
                                        {
                                            homeModel.upsertWord(uriTmp.DnsSafeHost, "EXTERNAL-LINK", 1);
                                        }

                                    }
                                }
                            }

                        }
                    }
                }
                else {

                    ViewBag.Message = (isValidUrl?"Unable to retrieve content from the provided website.":"Content to analyze is empty.");
                }
                return View("PostIndex", homeModel);
            }
            return View(homeModel);
        }
    }
}