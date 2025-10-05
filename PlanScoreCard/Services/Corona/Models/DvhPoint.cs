namespace CoronaDVH.Models
{
    public class DvhPoint
    {
        public DvhPoint(float dose, float volume)
        {
            Dose = dose;
            Volume = volume;
        }

        public float Dose { get; set; }
        public float Volume { get; set; }
    }
}