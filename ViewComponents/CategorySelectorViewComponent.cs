using Microsoft.AspNetCore.Mvc;

namespace _22DH113668_TranDaoDinhThuong.ViewComponents
{
    public class CategorySelectorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string? selectedCategory = null)
        {
            var categories = new List<string>
            {
                "Vợt",
                "Bóng",
                "Cầu",
                "Đệm",
                "Quần áo"
            };

            ViewBag.SelectedCategory = selectedCategory;
            return View(categories);
        }
    }
}