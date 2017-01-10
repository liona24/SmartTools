namespace SmartTools.Features
{
    public interface IFeatureDescripting
    {
        double this[int i] { get; }
        int Length { get; }
        MiniGL.Vec2 Position { get; }
    }
}
