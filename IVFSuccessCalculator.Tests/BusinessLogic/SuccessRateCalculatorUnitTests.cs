using FluentAssertions;
using IVFSuccessCalculator.BusinessLogic;
using IVFSuccessCalculator.Domain;
using Microsoft.Extensions.Options;

namespace IVFSuccessCalculator.Tests.BusinessLogic
{
    public class SuccessRateCalculatorUnitTests
    {
        private readonly SuccessRateCalculator _calculator;
        private readonly List<SuccessRateFormulaParameters> _formulaParameters = new();

        private const int DEFAULT_AGE = 20;
        private const int DEFAULT_HEIGHT = 68; // 5ft8
        private const int DEFAULT_WEIGHT = 150; // lbs

        public SuccessRateCalculatorUnitTests()
        {
            _calculator = new SuccessRateCalculator(Options.Create(_formulaParameters));
        }

        [Theory]
        [InlineData(true, true, true, 1)]
        [InlineData(true, true, false, 2)]
        [InlineData(true, false, true, 3)]
        [InlineData(true, false, false, 4)]
        [InlineData(false, true, true, 5)]
        [InlineData(false, false, true, 5)]
        [InlineData(false, true, false, 6)]
        [InlineData(false, false, false, 6)]
        public void Calculator_Selects_Appropriate_FormulaParams_BasedOn_SelectionCriteria(bool usingOwnEggs, bool usedIvfBefore, bool reasonForInfertilityKnown, int shouldSelectFormula)
        {
            // ARRANGE
            bool? dontCare_UsedIvf = null;

            _formulaParameters.AddRange([
                new SuccessRateFormulaParameters(usingOwnEggs: true, usedIvfBefore: true, reasonForInfertilityKnown: true) {
                    Intercept = -6.0
                },

                new SuccessRateFormulaParameters(usingOwnEggs: true, usedIvfBefore: true, reasonForInfertilityKnown: false) {
                    Intercept = -6.1
                },

                new SuccessRateFormulaParameters(usingOwnEggs: true, usedIvfBefore: false, reasonForInfertilityKnown: true) {
                    Intercept = -6.2
                },

                new SuccessRateFormulaParameters(usingOwnEggs: true, usedIvfBefore: false, reasonForInfertilityKnown: false) {
                    Intercept = -6.3
                },

                new SuccessRateFormulaParameters(usingOwnEggs: false, dontCare_UsedIvf, reasonForInfertilityKnown: true) {
                    Intercept = -6.4
                },

                new SuccessRateFormulaParameters(usingOwnEggs: false, dontCare_UsedIvf, reasonForInfertilityKnown: false) {
                    Intercept = -6.5
                },
             ]);

            var request = new SuccessRateCalculationParameters
            {
                Age = DEFAULT_AGE,
                Height = DEFAULT_HEIGHT,
                Weight = DEFAULT_WEIGHT,

                InfertilityDiagnosis = new(),

                UsingOwnEggs = usingOwnEggs,
                UsedIvfBefore = usedIvfBefore,
                ReasonForInfertilityKnown = reasonForInfertilityKnown,
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            var expectedScore = _formulaParameters[shouldSelectFormula - 1].Intercept;
            result.Should().Be(Sigmoid(expectedScore));
        }

        [Fact]
        public void Calculates_BasedOn_Age()
        {
            // ARRANGE
            var age = 20;

            var linearCoeff = 0.3;
            var powerCoeff = -0.0003;
            var powerFactor = 2.7;

            _formulaParameters.Add(new SuccessRateFormulaParameters(default, default, default)
            {
                AgeLinearCoefficient = linearCoeff,
                AgePowerCoefficient = powerCoeff,
                AgePowerFactor = powerFactor
            });

            var request = new SuccessRateCalculationParameters
            {
                Age = age,

                Height = DEFAULT_HEIGHT,
                Weight = DEFAULT_WEIGHT,
                InfertilityDiagnosis = new()
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            var expectedAgeScore = (linearCoeff * age) + (powerCoeff * Math.Pow(age, powerFactor));
            result.Should().Be(Sigmoid(expectedAgeScore));
        }

        [Fact]
        public void Calculates_BasedOn_Bmi()
        {
            // ARRANGE
            var weight = 150;

            const int FT = 12;
            var height = 5 * FT + 8;

            var linearCoeff = 0.05;
            var powerCoeff = -0.0009;
            var powerFactor = 2.0;

            _formulaParameters.Add(new SuccessRateFormulaParameters(default, default, default)
            {
                BmiLinearCoefficient = linearCoeff,
                BmiPowerCoefficient = powerCoeff,
                BmiPowerFactor = powerFactor
            });

            var request = new SuccessRateCalculationParameters
            {
                Age = DEFAULT_AGE,
                Height = height,
                Weight = weight,

                InfertilityDiagnosis = new()
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            var bmi = 22.8;
            var expectedBmiScore = (linearCoeff * bmi) + (powerCoeff * Math.Pow(bmi, powerFactor));

            result.Should().BeApproximately(Sigmoid(expectedBmiScore), 1e-5);
        }

        

        [Fact]
        public void Calculates_Based_On_Known_IvfFactors()
        {
            // ARRANGE
            _formulaParameters.Add(new SuccessRateFormulaParameters(default, default, true)
            {
                TubalFactor = 0.1,
                MaleInfertilityFactor = 0.2,
                EndometriosisFactor = 0.02,
                OvulatoryDisorderFactor = 0.25,
                DiminishedOvarianReserveFactor = -0.5,
                UterineFactor = -0.1,
                SupplementalFactor = -0.05,

                // This should not be factored into the calculation
                UnexplainedInfertilityFactor = 0.2
            });

            var request = new SuccessRateCalculationParameters
            {
                Age = DEFAULT_AGE,
                Height = DEFAULT_HEIGHT,
                Weight = DEFAULT_WEIGHT,

                ReasonForInfertilityKnown = true,
                InfertilityDiagnosis = new InfertilityFactors
                {
                    TubalFactor = true,
                    MaleFactorInf = true,
                    Endometriosis = true,
                    OvulatoryDisorder = true,
                    DiminishedOvarianReserve = true,
                    UterineFactor = true,
                    OtherReason = true
                }
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            var ivf_scores = 0.1 + 0.2 + 0.02 + 0.25 + -0.5 + -0.1 + -0.05;
            result.Should().Be(Sigmoid(ivf_scores));
        }

        [Fact]
        public void Calculates_Based_On_Unexplained_Infertility()
        {
            // ARRANGE
            _formulaParameters.Add(new SuccessRateFormulaParameters(default, default, true)
            {
                UnexplainedInfertilityFactor = 0.2,

                // These should not be factored into the calculation
                TubalFactor = 0.1,
                MaleInfertilityFactor = 0.2,
                EndometriosisFactor = 0.02,
                OvulatoryDisorderFactor = 0.25,
                DiminishedOvarianReserveFactor = -0.5,
                UterineFactor = -0.1,
                SupplementalFactor = -0.05,
            });

            var request = new SuccessRateCalculationParameters
            {
                Age = DEFAULT_AGE,
                Height = DEFAULT_HEIGHT,
                Weight = DEFAULT_WEIGHT,

                ReasonForInfertilityKnown = true,
                InfertilityDiagnosis = new InfertilityFactors
                {
                    UnexplainedInf = true
                }
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            var ivf_scores = 0.2;
            result.Should().Be(Sigmoid(ivf_scores));
        }

        [Fact]
        public void Calculates_Based_On_Unknown_Infertility()
        {
            // ARRANGE
            _formulaParameters.Add(new SuccessRateFormulaParameters(default, default, false)
            {
                // None of these should not be factored into the calculation
                TubalFactor = 0.1,
                MaleInfertilityFactor = 0.2,
                EndometriosisFactor = 0.02,
                OvulatoryDisorderFactor = 0.25,
                DiminishedOvarianReserveFactor = -0.5,
                UterineFactor = -0.1,
                SupplementalFactor = -0.05,
            });

            var request = new SuccessRateCalculationParameters
            {
                Age = DEFAULT_AGE,
                Height = DEFAULT_HEIGHT,
                Weight = DEFAULT_WEIGHT,

                ReasonForInfertilityKnown = false,
                InfertilityDiagnosis = new InfertilityFactors()
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            result.Should().Be(Sigmoid(0));
        }

        [Fact]
        public void Calculation_Incorporates_Formula_Intercept()
        {
            // ARRANGE
            _formulaParameters.Add(new SuccessRateFormulaParameters(default, default, false)
            {
                Intercept = -7.0
            });

            var request = new SuccessRateCalculationParameters
            {
                Age = DEFAULT_AGE,
                Height = DEFAULT_HEIGHT,
                Weight = DEFAULT_WEIGHT,

                ReasonForInfertilityKnown = false,
                InfertilityDiagnosis = new InfertilityFactors()
            };

            // ACT
            var result = _calculator.Calculate(request);

            // ASSERT
            result.Should().Be(Sigmoid(-7.0));
        }

        public double Sigmoid(double value) => Math.Exp(value) / (1 + Math.Exp(value));
    }
}
