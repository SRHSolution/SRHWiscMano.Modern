using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public static class InstantUtils
    {
        public static readonly Instant Epoch = new();

        public static Instant InstantFromMilliseconds(long milliseconds)
        {
            return Epoch + Duration.FromMilliseconds(milliseconds);
        }
    }
}
