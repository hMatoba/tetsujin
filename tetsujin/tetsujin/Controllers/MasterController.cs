using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Filters;
using tetsujin.Models;

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
            ViewBag.entries = Entry.GetRecentEntry((int)page, true);
            ViewBag.lastPage = System.Math.Ceiling((double)Entry.Count() / Entry.LIMIT);

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

        [Route("Edit/{id:int?}")]
        public IActionResult Edit(int? id)
        {
            ViewBag.id = id;

            Entry entry;
            if (id != null)
            {
                entry = Entry.GetEntry(ViewBag.id, true);
            }
            else
            {
                var timedelta = 9;
                entry = new Entry
                {
                    Tag = new List<string> { },
                    PublishDate = DateTime.Now.ToUniversalTime().AddHours(timedelta),
                    IsShown = true
                };
            }
            ViewBag.entry = entry;
            ViewBag.blobUrl = Startup.Configuration.GetSection("ConnectionStrings")["Blob"];

            return View(entry);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(Entry entry)
        {
            entry.Tag = !String.IsNullOrEmpty(entry._Tag) ? entry._Tag.Split(',').ToList<string>() : new List<string> { };
            entry.InsertOrUpdate();

            return View();
        }

        [Route("Profile/Edit")]
        public IActionResult EditProfile()
        {
            ViewBag.profile = Profile.Get();
            return View();
        }

        [HttpPost]
        [Route("Profile/Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult PostProfile()
        {
            Profile.Save(Request.Form["body"]);
            return Redirect("/Master");
        }

        [HttpGet]
        [Route("Images")]
        public IActionResult ImageForm()
        {
            return View();
        }

        [HttpPost]
        [Route("Images")]
        public async Task<string> SaveImagesAsync()
        {
            var files = Request.Form.Files.ToList();
            await BlobFiles.SaveImagesAsync(files);

            return "ok";
        }

        [HttpGet]
        [Route("Images/Info")]
        public async Task<ActionResult> GetImageInfoAsync()
        {
            var json = await BlobFiles.GetImageInfoAsync();
            return Content(json, "application/json");
        }
    }
}
