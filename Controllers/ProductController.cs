using _22DH113668_TranDaoDinhThuong.Models.ViewModels;
using _22DH113668_TranDaoDinhThuong.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SportProductApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly SportProductContext _context;

        public ProductController(SportProductContext context)
        {
            _context = context;
        }

        // GET: Product/Index
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .OrderBy(p => p.ProductId)
                .ToListAsync();

            return View(products);
        }

        // GET: Product/Details
        public async Task<IActionResult> Details()
        {
            var product = await _context.Products
                .OrderBy(p => p.ProductId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Không tìm thấy sản phẩm nào trong database.");
            }

            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                var firstProduct = await _context.Products
                    .OrderBy(p => p.ProductId)
                    .FirstOrDefaultAsync();

                if (firstProduct == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Edit), new { id = firstProduct.ProductId });
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductEditViewModel
            {
                ProductId = product.ProductId,
                NamePro = product.NamePro,
                Category = product.Category,
                ManufacturingDate = product.ManufacturingDate,
                DecriptionPro = product.DecriptionPro,
                Price = product.Price,
                ImagePro = product.ImagePro
            };

            return View(viewModel);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel viewModel, IFormFile? ImageFile)
        {
            if (id != viewModel.ProductId)
            {
                return NotFound();
            }

            // Custom validation cho Category
            if (!IsValidCategory(viewModel.Category))
            {
                ModelState.AddModelError("Category", "Loại sản phẩm phải là một trong các giá trị: Vợt, Bóng, Cầu, Đệm, Quần áo");
            }

            // Custom validation cho ManufacturingDate
            if (viewModel.ManufacturingDate > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("ManufacturingDate", "Ngày sản xuất không được là ngày tương lai");
            }

            // Custom validation cho Price - chỉ validate nếu có giá trị được nhập
            if (viewModel.Price.HasValue && viewModel.Price <= 0)
            {
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0");
            }
            if (viewModel.Price.HasValue && viewModel.Price > 1000000)
            {
                ModelState.AddModelError("Price", "Giá không được vượt quá 1,000,000 VND");
            }

            // Handle file upload
            string? imageFileName = null;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file hình ảnh (JPG, PNG, GIF, BMP)");
                }
                else if (ImageFile.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB");
                }
                else
                {
                    try
                    {
                        // Create uploads directory if it doesn't exist
                        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        // Generate unique filename
                        imageFileName = $"{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, imageFileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Set the image path for the product
                        viewModel.ImagePro = $"/uploads/{imageFileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageFile", $"Lỗi khi upload file: {ex.Message}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Model Binding - cập nhật dữ liệu từ form
                    // Các giá trị từ form sẽ giữ nguyên nếu user không thay đổi
                    product.NamePro = viewModel.NamePro;
                    product.Category = viewModel.Category;
                    product.ManufacturingDate = viewModel.ManufacturingDate;
                    product.DecriptionPro = viewModel.DecriptionPro;
                    
                    // Chỉ cập nhật Price nếu có giá trị mới, ngược lại giữ nguyên giá cũ
                    if (viewModel.Price.HasValue)
                    {
                        product.Price = viewModel.Price;
                    }
                    
                    // Update image only if new image is provided (either URL or uploaded file)
                    if (!string.IsNullOrEmpty(viewModel.ImagePro))
                    {
                        // Delete old image file if it exists and it's an uploaded file
                        if (!string.IsNullOrEmpty(product.ImagePro) && product.ImagePro.StartsWith("/uploads/"))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePro.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        
                        product.ImagePro = viewModel.ImagePro;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Details), new { id = product.ProductId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Clean up uploaded file if database update fails
                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageFileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    if (!ProductExists(viewModel.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(viewModel);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private bool IsValidCategory(string? category)
        {
            if (string.IsNullOrEmpty(category)) return false;
            var validCategories = new[] { "Vợt", "Bóng", "Cầu", "Đệm", "Quần áo" };
            return validCategories.Contains(category);
        }
    }
}