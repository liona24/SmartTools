namespace SmartTools.Features
{
    public interface IFeatureMatching
    {
        MiniGL.Vec2I[] Match(IFeatureDescripting[] set1, IFeatureDescripting[] set2, double thresh);
    }
}
