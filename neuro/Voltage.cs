namespace neuro
{
    public class Voltage
    {
        public double Value { get; set; }

        public Voltage(double value)
        {
            Value = value;
        }

        public Voltage()
        {
            Value = 0.0;
        }
    }
}