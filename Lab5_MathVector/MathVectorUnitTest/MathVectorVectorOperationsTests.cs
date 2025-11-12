using MathVectorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathVectorUnitTest
{
    [TestClass]
    public class MathVectorVectorOperationTests
    {
        private const double Tolerance = 1e-6;

        [TestMethod]
        public void Sum_WithValidVectors_ReturnsCorrectVector()
        {
            // Arrange
            var vector1 = new MathVector(1, 2, 3);
            var vector2 = new MathVector(4, 5, 6);
            var expected = new double[] { 5, 7, 9 };

            // Act
            var result = vector1.Sum(vector2);

            // Assert
            Assert.AreEqual(vector1.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Sum_WithDifferentDimensions_ThrowsException()
        {
            // Arrange
            var vector1 = new MathVector(1, 2, 3);
            var vector2 = new MathVector(1, 2);

            // Act & Assert
            _ = vector1.Sum(vector2);
        }

        [TestMethod]
        public void Multiply_ComponentWise_ReturnsCorrectVector()
        {
            // Arrange
            var vector1 = new MathVector(1, 2, 3);
            var vector2 = new MathVector(4, 5, 6);
            var expected = new double[] { 4, 10, 18 };

            // Act
            var result = vector1.Multiply(vector2);

            // Assert
            Assert.AreEqual(vector1.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        public void Divide_ComponentWise_ReturnsCorrectVector()
        {
            // Arrange
            var vector1 = new MathVector(4, 5, 6);
            var vector2 = new MathVector(1, 2, 3);
            var expected = new double[] { 4, 2.5, 2 };

            // Act
            var result = vector1.Divide(vector2);

            // Assert
            Assert.AreEqual(vector1.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        public void ScalarMultiply_ReturnsDotProduct()
        {
            // Arrange
            var vector1 = new MathVector(1, 2, 3);
            var vector2 = new MathVector(4, 5, 6);
            double expected = 32; // 1*4 + 2*5 + 3*6

            // Act
            var result = vector1.ScalarMultiply(vector2);

            // Assert
            Assert.AreEqual(expected, result, Tolerance);
        }

        [TestMethod]
        public void CalcDistance_ReturnsCorrectDistance()
        {
            // Arrange
            var vector1 = new MathVector(1, 2, 3);
            var vector2 = new MathVector(4, 5, 6);
            double expected = Math.Sqrt(27); // √[(4-1)² + (5-2)² + (6-3)²]

            // Act
            var result = vector1.CalcDistance(vector2);

            // Assert
            Assert.AreEqual(expected, result, Tolerance);
        }

        [TestMethod]
        public void CalcDistance_SameVector_ReturnsZero()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);

            // Act
            var result = vector.CalcDistance(vector);

            // Assert
            Assert.AreEqual(0, result, Tolerance);
        }
    }
}
