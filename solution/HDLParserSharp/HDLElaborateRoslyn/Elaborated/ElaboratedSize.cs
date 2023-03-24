using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.Elaborated
{
    public struct ElaboratedSize
    {
        public int MSB;
        public int LSB;
        public int Width => Math.Abs(MSB - LSB) + 1;

        public ElaboratedSize(int msb, int lsb)
        {
            MSB = msb;
            LSB = lsb;
        }

        public ElaboratedSize(int width)
        {
            LSB = 0;
            MSB = width - 1;
        }

        public override string ToString()
        {
            if (MSB == LSB)
            {
                return $"({Width})";
            }
            else
            {
                return $"[{MSB}:{LSB}]({Width})";
            }
        }
    }
}
