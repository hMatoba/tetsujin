using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Models;
using tetsujin.Filters;

namespace tetsujin.Controllers
{
    [AuthorizationFilter]
    [Route("Master")]
    public class MasterController : Controller
    {
        [Route("{page:int?}")]
        public IActionResult Index(int? page = 1)
        {
            page--;
            ViewBag.page = page;
            ViewBag.entries = Entry.GetRecentEntries((int)page, true);
            ViewBag.lastPage = System.Math.Ceiling((double)Entry.Count() / Entry.LIMIT);

            return View();
        }

        [Route("Edit/{id:int?}")]
        public IActionResult Edit(int? id)
        {
            Entry entry;
            if (id != null)
            {
                int entryId = id ?? 0;
                entry = Entry.GetEntry(entryId, true);
            }
            else
            {
                var timedelta = 9;
                entry = new Entry
                {
                    Tag = new List<string> { },
                    PublishDate = DateTime.Now.AddHours(timedelta),
                    IsShown = true
                };
            }
            ViewBag.entry = entry;
            return View(entry);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(Entry entry)
        {
            entry.InsertOrUpdate();
            return View();
        }

        [Route("Remove")]
        [HttpPost]
        public IActionResult Remove()
        {
            var ids = Request.Form["entryId[]"].Select((a) => Int32.Parse(a)).ToList();
            Entry.DeleteMany(ids);
            ViewBag.count = Request.Form["entryId[]"].Count;

            return View();
        }
    }
}
