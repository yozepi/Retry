﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retry
{
    public interface IDelay
    {
        Task WaitAsync(int tryCount);
    }
}
