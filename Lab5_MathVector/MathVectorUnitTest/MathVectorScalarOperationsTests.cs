using MathVectorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathVectorUnitTest
{
    [TestClass]
    public class MathVectorScalarOperationTests
    {
        private const double Tolerance = 1e-6;

        [TestMethod]
        public void SumNumber_WithPositiveNumber_ReturnsCorrectVector()
        {
            // Arrange
            var vector = new MathVector(-1, 2, 3);
            var expected = new double[] { 1, 4, 5 };

            // Act
            var result = vector.SumNumber(2);

            // Assert
            Assert.AreEqual(vector.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        public void SumNumber_WithNegativeNumber_ReturnsCorrectVector()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);
            var expected = new double[] { -1, 0, 1 };

            // Act
            var result = vector.SumNumber(-2);

            // Assert
            Assert.AreEqual(vector.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        public void MultiplyNumber_WithPositiveNumber_ReturnsCorrectVector()
        {
            // Arrange
            var vector = new MathVector(-1, 2, 3);
            var expected = new double[] { -3, 6, 9 };

            // Act
            var result = vector.MultiplyNumber(3);

            // Assert
            Assert.AreEqual(vector.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        public void MultiplyNumber_WithZero_ReturnsZeroVector()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);
            var expected = new double[] { 0, 0, 0 };

            // Act
            var result = vector.MultiplyNumber(0);

            // Assert
            Assert.AreEqual(vector.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        public void DivideNumber_WithPositiveNumber_ReturnsCorrectVector()
        {
            // Arrange
            var vector = new MathVector(-1, 2, 3);
            var expected = new double[] { -0.5, 1, 1.5 };

            // Act
            var result = vector.DivideNumber(2);

            // Assert
            Assert.AreEqual(vector.Dimensions, result.Dimensions);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], Tolerance);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void DivideNumber_ByZero_ThrowsException()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);

            // Act & Assert
            _ = vector.DivideNumber(0);
        }
    }
}
