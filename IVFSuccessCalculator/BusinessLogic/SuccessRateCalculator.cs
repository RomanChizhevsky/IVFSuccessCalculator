using IVFSuccessCalculator.Domain;
using Microsoft.Extensions.Options;

namespace IVFSuccessCalculator.BusinessLogic
{
    public interface ISuccessRateCalculator
    {
        double Calculate(SuccessRateCalculationRequest request);
    }

    public class SuccessRateCalculator : ISuccessRateCalculator
    {
        private readonly List<SuccessRateFormulaParameters> _formulaParameters;

        public SuccessRateCalculator(IOptions<List<SuccessRateFormulaParameters>> formulaParameters)
        {
            _formulaParameters = formulaParameters.Value;
        }

        public double Calculate(SuccessRateCalculationRequest request)
        {
            var formulaParams = _formulaParameters.First(f => f.CanBeUsed(request.UsingOwnEggs, request.UsedIvfBefore, request.ReasonForInfertilityKnown));
            var score = 0.0;

            score += formulaParams.Intercept;
            score += AgeScore(request.Age, formulaParams);
            score += BmiScore(Bmi(request.Weight, request.Height), formulaParams);

            var diagnosedWith = request.InfertilityDiagnosis;
            var ivfFactors = new[]
            {
                (diagnosedWith.TubalFactor, formulaParams.TubalFactor),
                (diagnosedWith.MaleFactorInf, formulaParams.MaleInfertilityFactor),
                (diagnosedWith.Endometriosis, formulaParams.EndometriosisFactor),
                (diagnosedWith.OvulatoryDisorder, formulaParams.OvulatoryDisorderFactor),
                (diagnosedWith.DiminishedOvarianReserve, formulaParams.DiminishedOvarianReserveFactor),
                (diagnosedWith.UterineFactor, formulaParams.UterineFactor),
                (diagnosedWith.OtherReason, formulaParams.SupplementalFactor),
                (diagnosedWith.UnexplainedInf, formulaParams.UnexplainedInfertilityFactor),
                
                (request.NumPriorPregnancies == 1, formulaParams.PriorPregnancyFactor),
                (request.NumPriorPregnancies > 1, formulaParams.MultiplePriorPregnanciesFactor),

                (request.NumLiveBirths == 1, formulaParams.LiveBirthFactor),
                (request.NumLiveBirths > 1, formulaParams.MultipleLiveBirthsFactor)
            };

            foreach (var (factor, weight) in ivfFactors)
            {
                // Ignore if this is not a contributing factor
                if ( !factor)
                    continue;

                score += weight;
            }

            var exp_score = Math.Exp(score);
            return exp_score / (1 + exp_score);
        }

        private static double AgeScore(int age, SuccessRateFormulaParameters formulaParams)
        {
            return PolyLinearScore(age, formulaParams.AgeParameters);
        }

        private static double BmiScore(double bmi, SuccessRateFormulaParameters formulaParams)
        {
            return PolyLinearScore(bmi, formulaParams.BmiParameters);
        }

        private static double PolyLinearScore(double metric, (double, double, double) parameters)
        {
            var (linearCoeff, powerCoeff, powerFactor) = parameters;
            return (linearCoeff * metric) + (powerCoeff * Math.Pow(metric, powerFactor));
        }

        private static double Bmi(int weight, int height)
        {
            return (weight / Math.Pow(height * 1.0, 2.0)) * 703;
        }
    }
}
