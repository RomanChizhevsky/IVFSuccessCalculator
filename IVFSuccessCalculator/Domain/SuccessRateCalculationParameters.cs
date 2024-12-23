namespace IVFSuccessCalculator.Domain
{
    public class SuccessRateCalculationParameters
    {
        public int Age { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }

        public required InfertilityFactors InfertilityDiagnosis { get; set; }
        public int NumPriorPregnancies { get; set; }
        public int NumLiveBirths { get; set; }

        public bool UsingOwnEggs { get; set; }
        public bool UsedIvfBefore { get; set; }
        public bool ReasonForInfertilityKnown { get; set; }
    }
}
