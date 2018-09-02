namespace Physics.Models
{
    public interface IProjectile
    {
        double Mass { get; }
        double Diameter { get; }
        double Area { get; }
        double X { get; }
        double Y { get; }
    }
}
