using System.ComponentModel.DataAnnotations;

namespace IVFSuccessCalculator.Models
{
    /// <summary>
    /// Represents the request payload for a success rate calculation.
    /// </summary>
    public class SuccessRateCalculationRequest
    {
        [Required]
        public int? Age { get; set; }

        [Required]
        public int? Weight { get; set; }

        [Required]
        public int? Height { get; set; }

        [Required]
        public InfertilityFactors? InfertilityDiagnosis { get; set; }

        [Required]
        public int? NumPriorPregnancies { get; set; }

        [Required]
        public int? NumLiveBirths { get; set; }

        [Required]
        public bool? UsingOwnEggs { get; set; }

        [Required]
        public bool? UsedIvfBefore { get; set; }

        [Required]
        public bool? ReasonForInfertilityKnown { get; set; }

        public SuccessRateCalculationParameters ToParameters()
        {
            // If model binding succeeds, and the request model is materialized,
            // then we guarantee all of the required parameters for the calculation
            // are set and non-null.

            return new SuccessRateCalculationParameters
            {
                Age = Age!.Value,
                Weight = Weight!.Value,
                Height = Height!.Value,
                InfertilityDiagnosis = InfertilityDiagnosis!,
                NumPriorPregnancies = NumPriorPregnancies!.Value,
                NumLiveBirths = NumLiveBirths!.Value,
                UsingOwnEggs = UsingOwnEggs!.Value,
                UsedIvfBefore = UsedIvfBefore!.Value,
                ReasonForInfertilityKnown = ReasonForInfertilityKnown!.Value,
            };
        }
    }
}
