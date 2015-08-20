using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EduChess
{
    public class ServiceController : Controller
    {
        public ActionResult Index()
        {
            var parser = new PgnParser();
            var game = parser.Parse(Server.MapPath("~/pgn/2.pgn"));
            return Json(game, JsonRequestBehavior.AllowGet);
        }
    }
}