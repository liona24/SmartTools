using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTools.Features
{
    public interface IScaleSpaceConstructing
    {
        ScaleSpace Construct(FrameF64 integralImage, int numOctaves);
    }
}
