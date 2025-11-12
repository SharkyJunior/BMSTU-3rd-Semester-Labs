using MathVectorNS;
using System.Runtime.Intrinsics;

namespace MathVectorUnitTest
{
    [TestClass]
        public class MathVectorOperatorsTests
        {
            private readonly double _tolerance = 0.0001;

            [TestMethod]
            public void OperatorPlus_TwoVectors_ReturnsCorrectSum()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(4, 5, 6);
                var expected = new MathVector(5, 7, 9);

                // Act
                var result = vector1 + vector2;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorPlus_VectorAndNumber_ReturnsCorrectSum()
            {
                // Arrange
                var vector = new MathVector(1, 2, 3);
                double number = 5;
                var expected = new MathVector(6, 7, 8);

                // Act
                var result = vector + number;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorPlus_DifferentDimensions_ThrowsArgumentException()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(1, 2);

                // Act & Assert
                Assert.ThrowsException<ArgumentException>(() => vector1 + vector2);
            }

            [TestMethod]
            public void OperatorMinus_TwoVectors_ReturnsCorrectDifference()
            {
                // Arrange
                var vector1 = new MathVector(5, 6, 7);
                var vector2 = new MathVector(1, 2, 3);
                var expected = new MathVector(4, 4, 4);

                // Act
                var result = vector1 - vector2;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorMinus_VectorAndNumber_ReturnsCorrectDifference()
            {
                // Arrange
                var vector = new MathVector(5, 6, 7);
                double number = 2;
                var expected = new MathVector(3, 4, 5);

                // Act
                var result = vector - number;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorMinus_DifferentDimensions_ThrowsArgumentException()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(1, 2);

                // Act & Assert
                Assert.ThrowsException<ArgumentException>(() => vector1 - vector2);
            }

            [TestMethod]
            public void OperatorMultiply_TwoVectors_ReturnsComponentWiseProduct()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(4, 5, 6);
                var expected = new MathVector(4, 10, 18);

                // Act
                var result = vector1 * vector2;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorMultiply_VectorAndNumber_ReturnsScaledVector()
            {
                // Arrange
                var vector = new MathVector(1, 2, 3);
                double number = 3;
                var expected = new MathVector(3, 6, 9);

                // Act
                var result = vector * number;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorMultiply_DifferentDimensions_ThrowsArgumentException()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(1, 2);

                // Act & Assert
                Assert.ThrowsException<ArgumentException>(() => vector1 * vector2);
            }

            [TestMethod]
            public void OperatorDivide_TwoVectors_ReturnsComponentWiseDivision()
            {
                // Arrange
                var vector1 = new MathVector(10, 20, 30);
                var vector2 = new MathVector(2, 4, 5);
                var expected = new MathVector(5, 5, 6);

                // Act
                var result = vector1 / vector2;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorDivide_VectorAndNumber_ReturnsScaledVector()
            {
                // Arrange
                var vector = new MathVector(10, 20, 30);
                double number = 2;
                var expected = new MathVector(5, 10, 15);

                // Act
                var result = vector / number;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorDivide_VectorByZero_ThrowsDivideByZeroException()
            {
                // Arrange
                var vector = new MathVector(1, 2, 3);
                double zero = 0;

                // Act & Assert
                Assert.ThrowsException<DivideByZeroException>(() => vector / zero);
            }

            [TestMethod]
            public void OperatorDivide_DifferentDimensions_ThrowsArgumentException()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(1, 2);

                // Act & Assert
                Assert.ThrowsException<ArgumentException>(() => vector1 / vector2);
            }

            [TestMethod]
            public void OperatorModulus_TwoVectors_ReturnsDotProduct()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(4, 5, 6);
                double expected = 32; // 1*4 + 2*5 + 3*6 = 4 + 10 + 18 = 32

                // Act
                var result = vector1 % vector2;

                // Assert
                Assert.AreEqual(expected, result, _tolerance);
            }

            [TestMethod]
            public void OperatorModulus_DifferentDimensions_ThrowsArgumentException()
            {
                // Arrange
                var vector1 = new MathVector(1, 2, 3);
                var vector2 = new MathVector(1, 2);

                // Act & Assert
                Assert.ThrowsException<ArgumentException>(() => vector1 % vector2);
            }

            [TestMethod]
            public void OperatorModulus_OrthogonalVectors_ReturnsZero()
            {
                // Arrange
                var vector1 = new MathVector(1, 0);
                var vector2 = new MathVector(0, 1);
                double expected = 0; // 1*0 + 0*1 = 0

                // Act
                var result = vector1 % vector2;

                // Assert
                Assert.AreEqual(expected, result, _tolerance);
            }

            [TestMethod]
            public void OperatorChain_ComplexExpression_ReturnsCorrectResult()
            {
                // Arrange
                var vector1 = new MathVector(2, 4, 6);
                var vector2 = new MathVector(1, 2, 3);
                double scalar = 2;
                var expected = new MathVector(6, 12, 18); // ((2,4,6) + (1,2,3)) * 2 = (3,6,9) * 2 = (6,12,18)

                // Act
                var result = (vector1 + vector2) * scalar;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }

            [TestMethod]
            public void OperatorChain_MultipleOperations_ReturnsCorrectResult()
            {
                // Arrange
                var vector1 = new MathVector(10, 20, 30);
                var vector2 = new MathVector(1, 2, 3);
                var expected = new MathVector(3, 6, 9); // ((10,20,30) - (1,2,3)) / 3 = (9,18,27) / 3 = (3,6,9)

                // Act
                var result = (vector1 - vector2) / 3;

                // Assert
                Assert.AreEqual(expected.Dimensions, result.Dimensions);
                for (int i = 0; i < expected.Dimensions; i++)
                {
                    Assert.AreEqual(expected[i], result[i], _tolerance);
                }
            }
        }
}
