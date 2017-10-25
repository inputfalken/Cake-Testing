using Microsoft.AspNetCore.Mvc;
using Store;

namespace Cake.Controllers {
    public class StoreController : Controller {
        public IActionResult Index() {
            return View("Index", new Coffee(84.2, Size.Large, "Latte", "Bean"));
        }
    }

}