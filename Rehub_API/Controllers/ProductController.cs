using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Rehub.DataAccess.Repository;
using Rehub.Models;
using Rehub.Models.ViewModels;
using static Rehub.Models.ViewModels.ProductVM;

namespace Rehub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(UnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region All Products

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Products")]
        public IActionResult GetAllProducts()
        {
            try
            {
                var productList = _unitOfWork.Product.GetAll(includeProperties: "Product,CoverType");
                return new JsonResult(productList)
                {
                    StatusCode = 200,
                };
            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }

        }
        #endregion

        #region Get Product

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Product")]
        public IActionResult GetProduct(int? id)
        {
            try
            {
                ProductVM.ProductModel product = new();

                if (id == null || id == 0)
                {
                    return BadRequest("Id is not valid");

                }
                else
                {
                    product.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                }

                if (product.Product == null)
                {
                    return new JsonResult("Product does not exist.")
                    {
                        StatusCode = 200,
                    };
                }

                Category category = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == product.Product.CategoryId);
                CoverType coverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == product.Product.CoverTypeId);
                product.Category = category.Name;
                product.CoverType = coverType.Name;

                return new JsonResult(product)
                {
                    StatusCode = 200,
                };
            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }

        }
        #endregion

        #region Insert A Product

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("Upsert")]
        public IActionResult GetProductById([FromBody] ProductList obj, IFormFile? file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var uploads = Path.Combine(wwwRootPath, @"images\products");
                        var extension = Path.GetExtension(file.FileName);

                        if (obj.Product.ImageUrl != null)
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }
                        obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                    }
                    if (obj.Product.Id == 0)
                    {
                        _unitOfWork.Product.Add(obj.Product);
                    }
                    else
                    {
                        _unitOfWork.Product.Update(obj.Product);
                    }
                    _unitOfWork.Save();
                }

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion

        #region Update Product

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("UpdateProduct")]
        public IActionResult UpdateProduct([FromBody] Product obj)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _unitOfWork.Product.Update(obj);
                    _unitOfWork.Save();

                    return new JsonResult("Product Updated successfully")
                    {
                        StatusCode = 200,
                    };
                }
                return Ok();

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion


        #region Delete Product
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpDelete, Route("Remove")]
        public IActionResult RemoveProduct(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return new JsonResult(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();

            return new JsonResult(new { success = true, message = "Delete Successful" });

        }
        #endregion

    }
}
