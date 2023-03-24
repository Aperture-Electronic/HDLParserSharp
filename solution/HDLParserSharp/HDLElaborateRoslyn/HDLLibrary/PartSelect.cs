using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.HDLLibrary
{
    public struct DownTo
    {
        public int MSB { get; }
        public int LSB { get; }

        public DownTo(int msb, int lsb)
        {
            MSB = msb;
            LSB = lsb;
        }
    }

    public struct PartSelect
    {
        public int Offset { get; }

        public int Length { get; }

        public bool IsPost = false;

        public bool IsPre => !IsPost;

        public PartSelect(int offset, int length, bool post)
        {
            Offset = offset;

            if (length == 0)
            {
                throw new Exception("The part select length can not be equal to 0");
            }

            Length = length;
            IsPost = post;
        }

        public static implicit operator DownTo(PartSelect partSelect)
        {
            if (partSelect.IsPost)
            {
                int msb = partSelect.Offset + partSelect.Length - 1;
                return new DownTo(msb, partSelect.Offset);
            }
            else
            {
                int lsb = partSelect.Offset - partSelect.Length + 1;
                return new DownTo(partSelect.Offset, lsb);
            }
        }
    }
}
