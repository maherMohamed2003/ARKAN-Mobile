using System;
using System.Collections.Generic;
using System.Text;

namespace ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs
{
    public class ResetPasswordDTO
    {
        public string newPassword { get; set; } = string.Empty;
        public int userId { get; set; }
    }
}
