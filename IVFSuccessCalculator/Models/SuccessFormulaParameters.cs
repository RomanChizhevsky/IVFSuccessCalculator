namespace IVFSuccessCalculator.Models
{
    public class SuccessRateFormulaParameters
    {
        private readonly SelectionCriteria _selectionCriteria;

        public double Intercept { get; set; }

        public double AgeLinearCoefficient { get; set; }
        public double AgePowerCoefficient { get; set; }
        public double AgePowerFactor { get; set; }

        public (double LinearCoeff, double PowerCoeff, double PowerFactor) AgeParameters => (AgeLinearCoefficient, AgePowerCoefficient, AgePowerFactor);

        public double BmiLinearCoefficient { get; set; }
        public double BmiPowerCoefficient { get; set; }
        public double BmiPowerFactor { get; set; }

        public (double LinearCoeff, double PowerCoeff, double PowerFactor) BmiParameters => (BmiLinearCoefficient, BmiPowerCoefficient, BmiPowerFactor);

        public double TubalFactor { get; set; }
        public double MaleInfertilityFactor { get; set; }
        public double EndometriosisFactor { get; set; }
        public double OvulatoryDisorderFactor { get; set; }
        public double DiminishedOvarianReserveFactor { get; set; }
        public double UterineFactor { get; set; }
        public double SupplementalFactor { get; set; }
        public double UnexplainedInfertilityFactor { get; set; }

        public double PriorPregnancyFactor { get; set; }
        public double MultiplePriorPregnanciesFactor { get; set; }

        public double LiveBirthFactor { get; set; }
        public double MultipleLiveBirthsFactor { get; set; }

        /// <summary>
        /// Initialize the success formula parameters, specifying selection critera params.
        /// </summary>
        public SuccessRateFormulaParameters(bool usingOwnEggs, bool? usedIvfBefore, bool reasonForInfertilityKnown)
        {
            _selectionCriteria = new SelectionCriteria 
            { 
                UsingOwnEggs = usingOwnEggs, 
                UsedIvfBefore = usedIvfBefore, 
                ReasonForInfertilityKnown = reasonForInfertilityKnown 
            };
        }

        public bool CanBeUsed(bool usingOwnEggs, bool usedIvfBefore, bool reasonForInfertilityKnown)
        {
            var criteria = new SelectionCriteria
            {
                UsingOwnEggs = usingOwnEggs,
                UsedIvfBefore = usedIvfBefore,
                ReasonForInfertilityKnown = reasonForInfertilityKnown
            };

            var notApplicable = default(bool?);

            // Check to see if prior IVF use is applicable for formula selection
            if (_selectionCriteria.UsedIvfBefore == notApplicable)
                return criteria with { UsedIvfBefore = notApplicable } == _selectionCriteria;

            return criteria == _selectionCriteria;
        }

        private record SelectionCriteria
        {
            public bool UsingOwnEggs { get; set; }
            public bool? UsedIvfBefore { get; set; }
            public bool ReasonForInfertilityKnown { get; set; }
        }
    }
}
