using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Sitecore_Interview.Models;
namespace Sitecore_Interview.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        [HttpGet]
        public ActionResult Index()
        {
            HomeModel homeModel = new HomeModel();
            return View(homeModel);
        }

        //POST: Data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HomeModel homeModel)
        {
            if (ModelState.IsValid)
            {
                //Check if URL to scrape option is selected
                if (homeModel.isUrlToScrape)
                {
                    //Check if the input is a valid URL
                    if (homeModel.isValidUrl(homeModel.textContent))
                    {
                        //Try to scrape the URL
                        string content = homeModel.getStringFromUrl(homeModel.textContent);

                        if (homeModel.isCountOfMetaKeywordsRequired)
                        {

                            Match matchMetaTagKeyword = Regex.Match(content, "\bmeta name=\"keywords\"\b", RegexOptions.IgnoreCase);
                            if (matchMetaTagKeyword.Success)
                            {

                            }
                            else
                            {
                                ViewBag.MessageMetaTagKeywords = "No meta tags found";
                            }
                        }

                        if (homeModel.isCountOfExternalLinksRequired)
                        {

                            MatchCollection matchColl = Regex.Matches(content, @"\ba href=\b", RegexOptions.IgnoreCase);

                        }


                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Malformed URL, please enter valid URL";
                        return View(homeModel);
                    }
                }
                else
                {
                    foreach (string stopWord in homeModel.filterStopWords.Split(','))
                    {
                        MatchCollection matchColl = Regex.Matches(homeModel.textContent, @"\b" + stopWord + @"\b", RegexOptions.IgnoreCase);
                        
                        //Update to datatable
                    }

                }

                return View("PostIndex", homeModel);
            }
            return View(homeModel);
        }
    }
}