namespace SmartTools.Features
{
    public interface IFeatureDescripting
    {
        double this[int i] { get; }
        int Length { get; }
        GraphicsUtility.Vec2 Position { get; }
    }
}
