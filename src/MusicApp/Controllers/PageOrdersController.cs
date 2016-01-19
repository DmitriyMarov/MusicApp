using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using MusicApp.Models;
using System.Security.Claims;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Http.Internal;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MusicApp.Controllers
{
    public class PageOrdersController : Controller
    {
        private ApplicationDbContext _context;

        public PageOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            int pageCount = 3; // TODO: по хорошему бы получать это значение
            var userId = User.GetUserId();
            var listHalf = _context.PageOrder.Where(x => x.UserID.ToString() == userId).Where(x => x.ContentSize == "Half").ToList();
            var listFull = _context.PageOrder.Where(x => x.UserID.ToString() == userId).Where(x => x.ContentSize == "Full").ToList();

            List<SelectListItem>[] dropDown = new List<SelectListItem>[9];
            for(int i = 0; i < dropDown.Length; i++)
            {
                dropDown[i] = new List<SelectListItem>();
            }

            int k = 0;
            var list = _context.PageOrder.Where(x => x.UserID.ToString() == userId).ToList();
            for (int i=1; i<= pageCount; i++)
            {
                var listPage = _context.PageOrder.Where(x => x.UserID.ToString() == userId).Where(x => x.PageNumber == i).ToList();
                if (listPage.Count == 2)
                {
                    var tempList = list.Where(x=>x.ContentSize=="Half").ToList();
                    foreach (var item in tempList)
                    {
                        dropDown[k].Add(new SelectListItem() { Text = item.PageContent });
                    }
                    dropDown[k].FirstOrDefault(x => x.Text == listPage[0].PageContent).Selected=true;
                    tempList = list.Where(x => x.ContentSize == "Half").ToList();
                    foreach (var item in tempList)
                    {
                        dropDown[k + 1].Add(new SelectListItem() { Text = item.PageContent });
                    }
                    dropDown[k+1].FirstOrDefault(x => x.Text == listPage[1].PageContent).Selected = true;
                    tempList = list.ToList();
                    foreach (var item in tempList)
                    {
                        dropDown[k + 2].Add(new SelectListItem() { Text = item.PageContent });
                    }
                    dropDown[k+2].FirstOrDefault(x => x.Text == "Ничего").Selected = true;
                }
                else if(listPage.Count == 1)
                {
                    var tempList = list.ToList();
                    foreach (var item in tempList)
                    {
                        dropDown[k+2].Add(new SelectListItem() { Text = item.PageContent });
                    }
                    dropDown[k+2].FirstOrDefault(x => x.Text == listPage[0].PageContent).Selected = true;
                    tempList = list.Where(x => x.ContentSize == "Half").ToList();
                    foreach (var item in tempList)
                    {
                        dropDown[k].Add(new SelectListItem() { Text = item.PageContent });
                        dropDown[k+1].Add(new SelectListItem() { Text = item.PageContent });
                    }
                    dropDown[k].FirstOrDefault(x => x.Text == "Ничего").Selected = true;
                    dropDown[k + 1].FirstOrDefault(x => x.Text == "Ничего").Selected = true;
                }
                k += 3;
            }

            for(int i = 0; i < pageCount*3; i++)
            {
                ViewData["dropDown"+(i+1).ToString()] = dropDown[i];
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string dropDown1, string dropDown2, string dropDown3, string dropDown4, string dropDown5, string dropDown6, string dropDown7, string dropDown8, string dropDown9)
        {
            //костыль, почему-то null передаётся
            if (dropDown5 == null)
                dropDown5 = "Ничего";
            //TODO: посмотреть, можно ли как-то удобней передать аргументы.
            var userId = User.GetUserId();
            //var list = _context.PageOrder.Where(x => x.UserID.ToString() == userId).ToList();
            //var listPage1 = list.Where(x=>x.PageNumber==1).ToList();

            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown1).PageNumber=1;
            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown2).PageNumber = 1;
            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown3).PageNumber = 1;

            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown4).PageNumber = 2;
            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown5).PageNumber = 2;
            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown6).PageNumber = 2;

            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown7).PageNumber = 3;
            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown8).PageNumber = 3;
            _context.PageOrder.Where(x => x.UserID.ToString() == userId).FirstOrDefault(x => x.PageContent == dropDown9).PageNumber = 3;

            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Создаёт правила отображения по умолчанию
        /// </summary>
        public void CreateDefaultOrder(string userID)
        {
            PageOrder pageOrder1 = new PageOrder()
            {
                ID = 0,
                UserID = userID,
                PageNumber = 1,
                PageContent = "Текст песни",
                ContentSize = "Half"
            };
            PageOrder pageOrder2 = new PageOrder()
            {
                ID = 0,
                UserID = userID,
                PageNumber = 1,
                PageContent = "Перевод песни",
                ContentSize = "Half"
            };
            PageOrder pageOrder3 = new PageOrder()
            {
                ID = 0,
                UserID = userID,
                PageNumber = 2,
                PageContent = "Аккорды",
                ContentSize = "Full"
            };
            PageOrder pageOrder4 = new PageOrder()
            {
                ID = 0,
                UserID = userID,
                PageNumber = 3,
                PageContent = "Блок ссылок",
                ContentSize = "Half"
            };
            PageOrder pageOrder5 = new PageOrder()
            {
                ID = 0,
                UserID = userID,
                PageNumber = 3,
                PageContent = "Ничего",
                ContentSize = "Half"
            };

            _context.PageOrder.Add(pageOrder1);
            _context.PageOrder.Add(pageOrder2);
            _context.PageOrder.Add(pageOrder3);
            _context.PageOrder.Add(pageOrder4);
            _context.PageOrder.Add(pageOrder5);
            _context.SaveChanges();
            return;
        }
    }
}
