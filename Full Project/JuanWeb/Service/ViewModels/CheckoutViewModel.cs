using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ViewModels
{
    public class CheckoutViewModel
    {
        public List<BasketItemViewModel> basketItemsVM { get; set; } = new List<BasketItemViewModel>();
        [Required]
        [StringLength(maximumLength: 100)]
        public string Email { get; set; }

        //[Required]
        //[StringLength(maximumLength:50 , MinimumLength = 5)]
        public string FullName { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string Addresses { get; set; }

        [StringLength(maximumLength: 100)]
        public string Aparment { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string City { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string Country { get; set; }



        [Required]
        [StringLength(maximumLength: 35)]
        public string Phone { get; set; }

        public decimal? TotalAmount { get; set; }
    }
}
