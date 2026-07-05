using System.ComponentModel.DataAnnotations;
using CustomerApp.Api.Validations;

namespace CustomerApp.Api.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [ValidEmail(ErrorMessage = "Email is not valid")]
        public string EmailId { get; set; }

        public string? PhoneNumber { get; set; }
        public string? MobilePhone { get; set; }
        public string? HomePhone { get; set; }
        public string? Address { get; set; }

        // Not stored in DB — computed for the grid only
        public string ContactNumber =>
            !string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber :
            !string.IsNullOrEmpty(MobilePhone) ? MobilePhone :
            HomePhone;
    }
}
