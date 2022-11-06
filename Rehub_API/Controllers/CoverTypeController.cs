using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Rehub.DataAccess.Repository;
using Rehub.Models;

namespace Rehub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoverTypeController : ControllerBase
    {

        private UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CoverTypeController(UnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region All CoverTypes

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("CoverTypes")]
        public IActionResult GetCoverTypes()
        {
            try
            {
                var CoverTypeList = _unitOfWork.CoverType.GetAll();
                return new JsonResult(CoverTypeList)
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

        #region Get CoverType

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("CoverType")]
        public IActionResult GetCoverType(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return BadRequest("Id is not valid.");
                }

                var CoverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

                if (CoverType == null)
                {
                    return BadRequest("Not Found.");
                }

                return new JsonResult(CoverType)
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

        #region Insert A CoverType

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("CreateCoverType")]
        public IActionResult CreateCoverType([FromBody] CoverType obj)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Data");
                }

                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();

                return new JsonResult("CoverType created successfully")
                {
                    StatusCode = 200,
                };
            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion

        #region Update CoverType

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("UpdateCoverType")]
        public IActionResult UpdateCoverType([FromBody] CoverType obj)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Data");
                }

                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();

                return new JsonResult("CoverType Updated successfully")
                {
                    StatusCode = 200,
                };

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion

        #region Delete CoverType
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpDelete, Route("RemoveCoverType")]
        public IActionResult RemoveCoverType(int? id)
        {
            try
            {
                var obj = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

                if (obj == null)
                {
                    return NotFound();
                }

                _unitOfWork.CoverType.Remove(obj);
                _unitOfWork.Save();

                return new JsonResult("CoverType removed successfully")
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
