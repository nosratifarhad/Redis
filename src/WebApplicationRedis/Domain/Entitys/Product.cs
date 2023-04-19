namespace WebApplicationRedis.Domain.Entitys
{
    public class Product
    {
        public int ProductId { get; private set; }

        public string ProductName { get; private set; }

        public string ProductTitle { get; private set; }

        public string? ProductDescription { get; private set; }

        public string? MainImageName { get; private set; }

        public string? MainImageTitle { get; private set; }

        public string? MainImageUri { get; private set; }

        public bool IsFreeDelivery { get; private set; }

        public bool IsExisting { get; private set; }

        public float? Price { get; private set; }

        public float? OffPrice { get; private set; }

        public int? Weight { get; private set; }

        #region Ctor

        public Product(
            string productName,
            string productTitle,
            string? productDescription,
            string? mainImageName,
            string? mainImageTitle,
            string? mainImageUri,
            bool isFreeDelivery,
            bool isExisting,
            int? weight)
        {
            ProductName = productName;
            ProductTitle = productTitle;
            ProductDescription = productDescription;
            MainImageName = mainImageName;
            MainImageTitle = mainImageTitle;
            MainImageUri = mainImageUri;
            IsFreeDelivery = isFreeDelivery;
            IsExisting = isExisting;
            Weight = weight;
        }

        public Product(
            int productId,
            string productName,
            string productTitle,
            string? productDescription,
            string? mainImageName,
            string? mainImageTitle,
            string? mainImageUri,
            bool isFreeDelivery,
            bool isExisting,
            int? weight)
        {
            ProductId = productId;
            ProductName = productName;
            ProductTitle = productTitle;
            ProductDescription = productDescription;
            MainImageName = mainImageName;
            MainImageTitle = mainImageTitle;
            MainImageUri = mainImageUri;
            IsFreeDelivery = isFreeDelivery;
            IsExisting = isExisting;
            Weight = weight;
        }

        #endregion Ctor

    }
}
