using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rehub.DataAccess.Repository;
using Rehub.Models;
using Rehub.Models.ViewModels;
using Rehub.Utility;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Rehub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public OrderController(UnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region Order Details

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Details")]
        public IActionResult GetDetails(int orderId)
        {
            try
            {
                OrderVM order = new OrderVM()
                {
                    OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
                };

                return new JsonResult(order)
                {
                    StatusCode = 200
                };

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }

        }
        #endregion

        #region All Orders

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet, Route("Orders")]
        public IActionResult GetOrders(string status)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders;


                if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
                {
                    orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
                }


                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");


                switch (status)
                {
                    case "pending":
                        orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                        break;
                    case "inprocess":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                        break;
                    case "completed":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                        break;
                    case "approved":
                        orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                        break;
                    default:
                        break;
                }


                return new JsonResult(orderHeaders);
            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }

        }
        #endregion

        #region Update Order Detail

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("UpdateOrderDetail")]
        public IActionResult UpdateOrderDetail([FromBody] OrderVM orderObj)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("Data is invalid");
                }

                var orderHEaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderObj.OrderHeader.Id, tracked: false);
                orderHEaderFromDb.Name = orderObj.OrderHeader.Name;
                orderHEaderFromDb.PhoneNumber = orderObj.OrderHeader.PhoneNumber;
                orderHEaderFromDb.StreetAddress = orderObj.OrderHeader.StreetAddress;
                orderHEaderFromDb.City = orderObj.OrderHeader.City;
                orderHEaderFromDb.State = orderObj.OrderHeader.State;
                orderHEaderFromDb.PostalCode = orderObj.OrderHeader.PostalCode;
                if (orderObj.OrderHeader.Carrier != null)
                {
                    orderHEaderFromDb.Carrier = orderObj.OrderHeader.Carrier;
                }
                if (orderObj.OrderHeader.TrackingNumber != null)
                {
                    orderHEaderFromDb.TrackingNumber = orderObj.OrderHeader.TrackingNumber;
                }
                _unitOfWork.OrderHeader.Update(orderHEaderFromDb);
                _unitOfWork.Save();

                return new JsonResult(orderHEaderFromDb)
                {
                    StatusCode = 200
                };

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion

        #region Start Processing
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("StartProcessing")]
        public IActionResult StartProcessing(int id)
        {

            _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusInProcess);
            _unitOfWork.Save();

            return new JsonResult("Success")
            {
                StatusCode = 200
            };
        }
        #endregion

        #region Ship Order
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("ShipOrder")]
        public IActionResult ShipOrder([FromBody] OrderVM order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Data.");
            }

            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == order.OrderHeader.Id, tracked: false);
            orderHeader.TrackingNumber = order.OrderHeader.TrackingNumber;
            orderHeader.Carrier = order.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            return new JsonResult("Order Shipped Successfully.")
            {
                StatusCode = 200
            };
        }
        #endregion

        #region CancelOrder
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("CancelOrder")]
        public IActionResult CancelOrder(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, tracked: false);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            return new JsonResult("Order Cancelled Successfully.")
            {
                StatusCode = 200
            };
        }
        #endregion




        #region Pay Now

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("PayNow")]
        public IActionResult PayNow([FromBody] OrderVM order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Data");
                }

                order.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == order.OrderHeader.Id, includeProperties: "ApplicationUser");
                order.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == order.OrderHeader.Id, includeProperties: "Product");

                //stripe settings 
                var domain = "https://localhost:7089/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={order.OrderHeader.Id}",
                    CancelUrl = domain + $"admin/order/details?orderId={order.OrderHeader.Id}",
                };

                foreach (var item in order.OrderDetail)
                {

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },

                        },
                        Quantity = item.Count,
                    };

                    options.LineItems.Add(sessionLineItem);

                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentID(order.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);

                return new StatusCodeResult(303);

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion

        #region Payment Confirmation

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost, Route("PaymentConfirmation")]
        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            try
            {
                if (orderHeaderid == null)
                {
                    return BadRequest("Invalid Id");
                }

                OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
                if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);
                    //check the stripe status
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                        _unitOfWork.Save();
                    }
                }

                return new JsonResult(orderHeader)
                {
                    StatusCode = 200
                };

            }

            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            return null;
        }
        #endregion

    }
}
