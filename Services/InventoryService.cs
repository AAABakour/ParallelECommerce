using ParallelECommerce.Models;

namespace ParallelECommerce.Services;

public class InventoryService
{
    private readonly Dictionary<int, Product> _products = new();

    // هذا القفل سنستخدمه في نسخة After لحماية المخزون من Race Condition
    private readonly object _stockLock = new();

    public InventoryService()
    {
        _products[1] = new Product
        {
            Id = 1,
            Name = "Gaming Laptop",
            StockQuantity = 10
        };
    }

    public Product? GetProduct(int productId)
    {
        if (_products.TryGetValue(productId, out var product))
        {
            return product;
        }

        return null;
    }

    public void ResetStock(int productId, int quantity)
    {
        if (_products.TryGetValue(productId, out var product))
        {
            product.StockQuantity = quantity;
        }
    }

    // BEFORE: نسخة غير آمنة، فيها Race Condition
    public async Task<bool> PurchaseBeforeAsync(int productId, int quantity)
    {
        var product = GetProduct(productId);

        if (product is null)
        {
            return false;
        }

        if (product.StockQuantity < quantity)
        {
            return false;
        }

        // تأخير مقصود حتى نكبر احتمال حدوث Race Condition
        await Task.Delay(100);

        product.StockQuantity -= quantity;

        return true;
    }

    // AFTER: نسخة آمنة باستخدام lock
    public bool PurchaseAfter(int productId, int quantity)
    {
        lock (_stockLock)
        {
            var product = GetProduct(productId);

            if (product is null)
            {
                return false;
            }

            if (product.StockQuantity < quantity)
            {
                return false;
            }

            product.StockQuantity -= quantity;

            return true;
        }
    }
}