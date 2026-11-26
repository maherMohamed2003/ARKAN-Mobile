using System;
using System.Collections.Generic;
using System.Text;

namespace ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs
{
    public class CheckCodeRequestDTO
    {
        public string code { get; set; } = string.Empty;
        public int userId { get; set; }
    }
}
