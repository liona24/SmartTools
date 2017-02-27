
namespace SmartTools.Features
{
    public interface IScaleSpaceConstructing
    {
        ScaleSpace Construct(FrameF64 integralImage, int numOctaves);
    }
}
