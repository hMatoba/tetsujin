using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Models;

namespace tetsujin.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [Route("")]
        public async Task<IActionResult> IndexAsync()
        {
            var page = 1;
            var pageSkip = page - 1 ;
            var entries = await Entry.GetRecentEntriesAsync(pageSkip);

            ViewBag.page = page;
            ViewBag.pagePath = "/Page";
            var count = await Entry.CountAsync();
            ViewBag.lastPage = System.Math.Ceiling((double)count / Entry.LIMIT);
            ViewBag.entries = entries;
            return View("Index");
        }

        [Route("Page/{page:int?}")]
        public async Task<IActionResult> PageIndexAsync(int page = 1)
        {
            var pageSkip = page - 1;
            var entries = await Entry.GetRecentEntriesAsync(pageSkip);

            if (entries.Count > 0)
            {
                ViewBag.page = page;
                ViewBag.pagePath = "/Page";
                var count = await Entry.CountAsync();
                ViewBag.lastPage = System.Math.Ceiling((double)count / Entry.LIMIT);
                ViewBag.entries = entries;
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Filter/Date/{date}/")]
        public async Task<IActionResult> IndexDateFilteredAsync(string date)
        {
            var d = date.Split('-').Select(Int32.Parse).ToList();
            var year = d[0];
            var month = d[1];

            var entries = await Entry.FilterByMonthAsync(year, month);
            if (entries.Count > 0)
            {
                ViewBag.entries = entries;
                ViewBag.k = date;
                return View("Month");
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Filter/Tag/{tag}/{page:int?}")]
        public async Task<IActionResult> IndexTagFilteredAsync(string tag, int page = 1)
        {
            var tagList = new List<string> { tag };

            var pageSkip = page - 1;
            var entries = await Entry.GetSameTagEntryAsync(tagList, pageSkip);
 
            if (entries.Count > 0)
            {
                ViewBag.page = page;

                var escapedTag = Uri.EscapeDataString(tag);
                ViewBag.pagePath = $"/Filter/Tag/{escapedTag}";
                ViewBag.entries = entries;
                var count = await Entry.CountFilteredAsync(tagList, false);
                ViewBag.lastPage = System.Math.Ceiling((double)count / Entry.LIMIT);
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Article/{id:int}")]
        public async Task<IActionResult> ShowEntryAsync(int id)
        {
            var entry = await Entry.GetEntryAsync(id);
            if (entry != null)
            {
                ViewBag.entry = entry;
                ViewBag.id = id;
                return View("Article");
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("sitemap.xml")]
        public IActionResult Sitemap()
        {
            HttpContext.Response.ContentType = "text/xml";
            ViewBag.baseUrl = "https://" + HttpContext.Request.Host.Value.ToString() + "/";
            return View();
        }

        [Route("{*path}")]
        public IActionResult Error()
        {
            return View("NotFound");
        }
    }
}
