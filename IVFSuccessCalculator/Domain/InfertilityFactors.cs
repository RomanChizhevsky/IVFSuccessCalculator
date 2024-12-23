namespace IVFSuccessCalculator.Domain
{
    public record InfertilityFactors
    {
        public bool TubalFactor { get; set; }
        public bool MaleFactorInf { get; set; }
        public bool Endometriosis { get; set; }
        public bool OvulatoryDisorder { get; set; }
        public bool DiminishedOvarianReserve { get; set; }
        public bool UterineFactor { get; set; }
        public bool OtherReason { get; set; }
        public bool UnexplainedInf { get; set; }

        public static InfertilityFactors None => new InfertilityFactors();
    }
}
