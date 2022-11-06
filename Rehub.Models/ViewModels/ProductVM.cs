using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rehub.Models.ViewModels
{
   
    public class ProductVM
    {
        public class ProductModel
        {
            public Product Product { get; set; }
            public string Category { get; set; }
            public string CoverType { get; set; }
        }
        public class ProductList
        {
            public Product Product { get; set; }
            public List<string> CategoryList { get; set; }
            public List<string> CoverTypeList { get; set; }
        }
    }
}
