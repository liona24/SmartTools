namespace SmartTools.Features
{
    public interface IFeatureExtracting
    {
        IFeatureDescripting[] Extract(FrameF64 integralImage);
    }
}
