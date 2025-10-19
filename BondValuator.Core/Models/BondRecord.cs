namespace BondValuator.Core.Models
{
    public class BondRecord
    {
        public string BondID { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Rate { get; set; } = string.Empty;
        public decimal FaceValue { get; set; }
        public string PaymentFrequency { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double YearsToMaturity { get; set; }
        public double DiscountFactor { get; set; }
        public string DeskNotes { get; set; } = string.Empty;
    }
}
