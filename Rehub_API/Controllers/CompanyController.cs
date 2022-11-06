using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Rehub.DataAccess.Repository;
using Rehub.Models;

namespace Rehub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {

        private UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CompanyController(UnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region All Companies

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Categories")]
        public IActionResult GetCategories()
        {
            try
            {
                var CompanyList = _unitOfWork.Company.GetAll();
                return new JsonResult(CompanyList)
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

        #region Get Company

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Company")]
        public IActionResult GetCompany(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return BadRequest("Id is not valid.");
                }

                var Company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

                if (Company == null)
                {
                    return BadRequest("Not Found.");
                }
                
                return new JsonResult(Company)
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

        #region Create Company

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("CreateCompany")]
        public IActionResult CreateCompany([FromBody] Company obj)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("Ivalid data.");
                }

                _unitOfWork.Company.Add(obj);
                _unitOfWork.Save();

                return new JsonResult("Company Created Succseesfully")
                {
                    StatusCode = 200
                };

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return Ok();
        }
        #endregion

        #region Update Company

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("UpdateCompany")]
        public IActionResult UpdateCompany([FromBody] Company obj)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _unitOfWork.Company.Update(obj);
                    _unitOfWork.Save();

                    return new JsonResult("Company Updated successfully")
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


        #region Delete Company
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpDelete, Route("Remove")]
        public IActionResult RemoveCompany(int? id)
        {
            try
            {
                var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

                if (obj == null)
                {
                    return BadRequest("Not Found");
                }

                _unitOfWork.Company.Remove(obj);
                _unitOfWork.Save();

                return new JsonResult("Company removed successfully")
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
