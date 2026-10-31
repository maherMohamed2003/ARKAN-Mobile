using System;
using System.Collections.Generic;
using System.Text;

namespace ArkanMaui.DTOs
{
    public class LoginResponseDTO
    {
            public Value value { get; set; }
            public bool isSuccess { get; set; }
            public bool isFailure { get; set; }
            public object error { get; set; }
        

        public class Value
        {
            public string accessToken { get; set; }
            public string refreshToken { get; set; }
            public DateTime accessTokenExpiration { get; set; }
            public int userId { get; set; }
            public object userName { get; set; }
            public string fullName { get; set; }
            public string email { get; set; }
            public string role { get; set; }
        }

    }
}
