﻿using System;
using System.Collections.Generic;
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
        public IActionResult Index(int page = 1)
        {
            page--;
            var entries = Entry.GetRecentEntry(page);

            if (entries.Count > 0)
            {
                ViewBag.page = page;
                ViewBag.pagePath = "/Page";
                ViewBag.lastPage = System.Math.Ceiling((double)Entry.Count() / Entry.LIMIT);
                ViewBag.entries = entries;
                return View();
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Page/{page:int?}")]
        public IActionResult PageIndex(int page = 1)
        {
            page--;
            var entries = Entry.GetRecentEntry(page);

            if (entries.Count > 0)
            {
                ViewBag.page = page;
                ViewBag.pagePath = "/Page";
                ViewBag.lastPage = System.Math.Ceiling((double)Entry.Count() / Entry.LIMIT);
                ViewBag.entries = entries;
                return View("Index");
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Filter/Date/{date}/")]
        public IActionResult IndexDateFiltered(string date)
        {
            var d = date.Split('-').Select(Int32.Parse).ToList();
            var year = d[0];
            var month = d[1];

            var entries = Entry.FilterByMonth(year, month);
            if (entries.Count > 0)
            {
                ViewBag.entries = entries;
                ViewBag.k = date;
                return View();
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Filter/Tag/{tag}/{page:int?}")]
        public IActionResult IndexTagFiltered(string tag, int page = 1)
        {
            var tagList = new List<string> { tag };

            page--;
            var entries = Entry.GetSameTagEntry(tagList, page);

            if (entries.Count > 0)
            {
                ViewBag.page = page;

                var escapedTag = Uri.EscapeDataString(tag);
                ViewBag.pagePath = $"/Filter/Tag/{escapedTag}";
                ViewBag.entries = entries;
                ViewBag.lastPage = System.Math.Ceiling((double)Entry.CountFiltered(tagList, false) / Entry.LIMIT);
                return View("Index");
            }
            else
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
        }

        [Route("Entry/{id:int}/{title?}")]
        public IActionResult ShowEntry(int id)
        {
            var entry = Entry.GetEntry(id);
            if (entry != null)
            {
                ViewBag.entry = entry;
                return View();
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
            ViewBag.baseUrl = Startup.Configuration.GetSection("ConnectionStrings")["Url"];
            return View();
        }

        [Route("Health/Check")]
        public IActionResult CheckHealth()
        {
            return Json(new Dictionary<string, string> { { "status", "healthy" } });
        }

        [Route("{*path}")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
