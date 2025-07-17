using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _22DH113668_TranDaoDinhThuong.Models;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(100, ErrorMessage = "Tên không vượt quá 100 ký tự")]
    public string? NamePro { get; set; }

    [StringLength(500)]
    public string? DecriptionPro { get; set; }

    public string? Category { get; set; }

    [Range(1, 1000000, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal? Price { get; set; }

    public string? ImagePro { get; set; }

    public DateOnly ManufacturingDate { get; set; }
}
