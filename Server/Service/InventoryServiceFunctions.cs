using Grpc.Core;
using InventoryService.Proto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using static InventoryService.Proto.Inventory;

namespace InventoryService.Services
{
    public class InventoryServiceFunctions : InventoryBase
    {
        // Use ConcurrentBag for thread-safe operations
        private static readonly ConcurrentBag<Product> Products = new ConcurrentBag<Product>();
        private readonly ILogger<InventoryServiceFunctions> _logger;
        private readonly IApiKeyService _apiKeyService;

        public InventoryServiceFunctions(ILogger<InventoryServiceFunctions> logger, IApiKeyService apiKeyService)
        {
            _logger = logger;
            _apiKeyService = apiKeyService;
        }

        public override Task<ProductExistenceResponse> GetProductById(ProductIdRequest request, ServerCallContext context)
        {
            bool exists = Products.Any(p => p.Id == request.Id);
            return Task.FromResult(new ProductExistenceResponse { Exists = exists });
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override Task<AddProductResponse> AddProduct(Product request, ServerCallContext context)
        {
            if (!_apiKeyService.Authenticate())
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Task.FromResult(new AddProductResponse { Message = "Unauthorized" });
            }

            Products.Add(request);
            _logger.LogInformation($"Product added: {request.Id}");
            return Task.FromResult(new AddProductResponse { Message = "Product added successfully" });
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override Task<UpdateProductResponse> UpdateProduct(Product request, ServerCallContext context)
        {
            if (!_apiKeyService.Authenticate())
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Task.FromResult(new UpdateProductResponse { Message = "Unauthorized" });
            }

            var product = Products.FirstOrDefault(p => p.Id == request.Id);
            if (product != null)
            {
                product.Title = request.Title;
                product.Price = request.Price;
                product.Quantity = request.Quantity;
                product.Category = request.Category;
                product.ExpireDate = request.ExpireDate;
                _logger.LogInformation($"Product updated: {request.Id}");
                return Task.FromResult(new UpdateProductResponse { Message = "Product updated successfully" });
            }
            else
            {
                _logger.LogWarning($"Product not found: {request.Id}");
                return Task.FromResult(new UpdateProductResponse { Message = "Product not found" });
            }
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override async Task<BulkProductResponse> AddBulkProducts(IAsyncStreamReader<Product> requestStream, ServerCallContext context)
        {
            if (!_apiKeyService.Authenticate())
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return new BulkProductResponse { InsertedCount = 0, Message = "Unauthorized" };
            }

            int count = 0;
            await foreach (var product in requestStream.ReadAllAsync())
            {
                Products.Add(product);
                count++;
                _logger.LogInformation($"Product added in bulk: {product.Id}");
            }
            return new BulkProductResponse { InsertedCount = count, Message = "Bulk products added successfully" };
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override async Task GetProductReport(ProductReportRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
        {
            if (!_apiKeyService.Authenticate())
            {
                _logger.LogWarning("Unauthorized access attempt.");
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Unauthorized"));
            }

            List<Product> data = Products.ToList();
            try
            {
                if (request.PriceOrder)
                {
                    foreach (var product in data.OrderBy(p => p.Price))
                    {
                        await responseStream.WriteAsync(product);
                    }
                }
                else if (request.CategoryFilter != Category.Not)
                {
                    foreach (var product in data.Where(p => p.Category == request.CategoryFilter))
                    {
                        await responseStream.WriteAsync(product);
                    }
                }
                else
                {
                    foreach (var product in data)
                    {
                        await responseStream.WriteAsync(product);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating product report.");
                throw;
            }
        }
    }
}
