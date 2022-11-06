using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Rehub.DataAccess.Repository;
using Rehub.Models;

namespace Rehub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CategoryController(UnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region All Categorys

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Categories")]
        public IActionResult GetCategories()
        {
            try
            {
                var CategoryList = _unitOfWork.Category.GetAll();
                return new JsonResult(CategoryList)
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

        #region Get Category

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Category")]
        public IActionResult GetCategory(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return BadRequest("Id is not valid.");
                }

                var category = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

                if (category == null)
                {
                    return BadRequest("Not Found.");
                }

                return new JsonResult(category)
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

        #region Insert A Category

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("Upsert")]
        public IActionResult CreateCategory([FromBody] Category obj)
        {
            try
            {
                if (obj.Name == obj.DisplayOrder.ToString())
                {
                    return BadRequest("The DisplayOrder cannot exactly match the Name.");

                }

                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Update(obj);
                    _unitOfWork.Save();

                    return new JsonResult("Category created successfully")
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

        #region Update Category

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("UpdateCategory")]
        public IActionResult UpdateCategory([FromBody] Category obj)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Update(obj);
                    _unitOfWork.Save();

                    return new JsonResult("Category Updated successfully")
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


        #region Delete Category
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpDelete, Route("Remove")]
        public IActionResult RemoveCategory(int? id)
        {
            try
            {
                var obj = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

                if (obj == null)
                {
                    return NotFound();
                }

                _unitOfWork.Category.Remove(obj);
                _unitOfWork.Save();

                return new JsonResult("Category removed successfully")
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

    }
}
