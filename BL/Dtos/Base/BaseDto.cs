using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Dtos.Base
{
    public class BaseDto
    {
        public Guid Id { get; set; }
        public int CurrentState { get; set; }
    }
}
