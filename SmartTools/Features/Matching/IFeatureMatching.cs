namespace SmartTools.Features
{
    public interface IFeatureMatching
    {
        GraphicsUtility.Vec2I[] Match(IFeatureDescripting[] set1, IFeatureDescripting[] set2, double thresh);
    }
}
