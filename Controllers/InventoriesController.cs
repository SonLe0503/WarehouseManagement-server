using Microsoft.AspNetCore.Mvc;

namespace warehouseManagement.Controllers
{
    public class InventoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
