namespace WebApplicationRedis.ViewModels.ProductViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductTitle { get; set; }

        public string? ProductDescription { get; set; }

        public string? MainImageName { get; set; }

        public string? MainImageTitle { get; set; }

        public string? MainImageUri { get; set; }

        public bool IsFreeDelivery { get; set; }

        public bool IsExisting { get; set; }

        public int? Weight { get; set; }
    }
}
