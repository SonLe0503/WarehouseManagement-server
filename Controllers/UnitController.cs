using Microsoft.AspNetCore.Mvc;

namespace warehouseManagement.Controllers
{
    public class UnitController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
