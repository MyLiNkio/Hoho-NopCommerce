namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product type
    /// </summary>
    public enum ProductType
    {
        //Ввести додаткові типи продуктів:
        //Сервіс-емоція, Мультисертифікат, НомінальнийСертифікат, Цифровий продукт, Підписка, Упаковка, і т.д.

        /// <summary>
        /// Simple product is the product which can be just added to the shoping cart and calculated as usually.
        /// </summary>
        SimpleProduct = 5,

        /// <summary>
        /// Grouped (product with variants)
        /// </summary>
        GroupedProduct = 10,

        //HOHOModification

        /// <summary>
        /// It is a Product or better to say a Service, which can't be sold separately, only binded to a Multicertificate)
        /// </summary>
        ApplyToMulticertificate = 15,

        /// <summary>
        /// Multicertificate (It is a phisical or electronic card. During checkout process we bind services to it)
        /// </summary>
        Multicertificate = 20,
        ElectronicMulticertificate = 21,

        /// <summary>
        /// Products with that type are used to package a phisical certificate.
        /// </summary>
        PackagingForMulticertificate = 25,
    }
}