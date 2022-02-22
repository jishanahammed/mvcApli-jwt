using System;
using System.Collections.Generic;

namespace mvcApli.Models
{
    public class UserManagerResponse
    {
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public bool IsSuccess { get; set; }
        public string RoleName { get; set; }
        public DateTime? ExpireDate { get; set; }
    }
}
