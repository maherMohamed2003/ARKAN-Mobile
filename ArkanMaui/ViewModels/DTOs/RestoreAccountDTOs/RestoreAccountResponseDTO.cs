using System;
using System.Collections.Generic;
using System.Text;

namespace ArkanMaui.ViewModels.DTOs.RestoreAccountDTOs
{
    public class RestoreAccountResponseDTO
    {
        public Value value { get; set; }
        public bool isSuccess { get; set; }
        public bool isFailure { get; set; }
        public object? error { get; set; }

        public class Value
        {
            public int UserId { get; set; }
            public List<RestoreMethodDTO> RestoreMethodResponseDTOs { get; set; } = new();
        }

        public class RestoreMethodDTO
        {
            public string MethodName { get; set; } = string.Empty;   // Telegram / Whatsapp / Gmail
            public string MethodTarget { get; set; } = string.Empty; // 01*******97
        }
    }

}
