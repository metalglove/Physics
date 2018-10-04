namespace Physics.Models
{
    public class EV3Message
    {
        public string Distance { get; set; }
        public string Angle { get; set; }

        public EV3Message(string distance, string angle)
        {
            Distance = distance;
            Angle = angle;
        }
    }
}
