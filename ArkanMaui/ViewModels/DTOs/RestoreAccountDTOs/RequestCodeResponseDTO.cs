using System;
using System.Collections.Generic;
using System.Text;

namespace ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs
{
    public class RequestCodeResponseDTO
    {
        public string? Value { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsFailure { get; set; }
        public object? Error { get; set; }
    }
}
