namespace WebApplicationRedis.InputModels.ProductInputModels
{
    public class CreateProductInputModel
    {
        public string ProductName { get; set; }

        public string ProductTitle { get; set; }

        public string? ProductDescription { get; set; }

        public ProductCategory? ProductCategory { get; set; }

        public string? MainImageName { get; set; }

        public string? MainImageTitle { get; set; }

        public string? MainImageUri { get; set; }

        public ProductColor? Color { get; set; }

        public bool IsFreeDelivery { get; set; }

        public bool IsExisting { get; set; }

        public int? Weight { get; set; }

    }
}
