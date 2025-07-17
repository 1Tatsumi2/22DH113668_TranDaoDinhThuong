using System.ComponentModel.DataAnnotations;

namespace _22DH113668_TranDaoDinhThuong.Models.ViewModels
{
    public class ProductEditViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [Display(Name = "Tên sản phẩm")]
        [StringLength(100, ErrorMessage = "Tên không vượt quá 100 ký tự")]
        public string? NamePro { get; set; }

        [Required(ErrorMessage = "Loại sản phẩm là bắt buộc")]
        [Display(Name = "Loại sản phẩm")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Ngày sản xuất là bắt buộc")]
        [Display(Name = "Ngày sản xuất")]
        [DataType(DataType.Date)]
        public DateOnly ManufacturingDate { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? DecriptionPro { get; set; }

        [Display(Name = "Giá")]
        public decimal? Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImagePro { get; set; }
    }
}
