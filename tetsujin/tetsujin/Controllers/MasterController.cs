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
        public async Task<IActionResult> IndexAsync(int? page = 1)
        {
            page--;
            ViewBag.page = page;
            ViewBag.entries = await Entry.GetRecentEntriesAsync((int)page, true);
            var count = await Entry.CountAsync();
            ViewBag.lastPage = System.Math.Ceiling((double)count / Entry.LIMIT);

            return View("Index");
        }

        [Route("Edit/{id:int?}")]
        public async Task<IActionResult> EditAsync(int? id)
        {
            Entry entry;
            if (id != null)
            {
                int entryId = id ?? 0;
                entry = await Entry.GetEntryAsync(entryId, true);
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
        public async Task<IActionResult> EditPostAsync(Entry entry)
        {
            await entry.InsertOrUpdateAsync();
            return View("EditPost");
        }

        [Route("Remove")]
        [HttpPost]
        public async Task<ActionResult> RemoveAsync()
        {
            var ids = Request.Form["entryId[]"].Select((a) => Int32.Parse(a)).ToList();
            await Entry.DeleteManyAsync(ids);
            ViewBag.count = Request.Form["entryId[]"].Count;

            return View("Remove");
        }

        [Route("Profile/Edit")]
        public async Task<IActionResult> EditProfileAsync()
        {
            ViewBag.profile = await Profile.GetAsync();
            return View("EditProfile");
        }

        [HttpPost]
        [Route("Profile/Edit")]
        public async Task<IActionResult> PostProfileAsync()
        {
            await Profile.SaveAsync(Request.Form["body"]);
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
            await BlobFile.SaveImagesAsync(files);

            return "ok";
        }

        [HttpGet]
        [Route("Images/Info")]
        public async Task<ActionResult> GetImageInfoAsync()
        {
            var json = await BlobFile.GetImageInfoAsync();
            return Content(json, "application/json");
        }
    }
}
