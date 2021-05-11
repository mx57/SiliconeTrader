using System.ComponentModel.DataAnnotations;

namespace SiliconeTrader.UI.Models
{
    public class LoginViewModel : BaseViewModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
