using MathVectorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathVectorUnitTest
{
    [TestClass]
    public class MathVectorPropertyTests
    {
        private const double Tolerance = 1e-6;

        [TestMethod]
        public void Dimensions_ReturnsCorrectValue()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3, 4, 5);

            // Act & Assert
            Assert.AreEqual(5, vector.Dimensions);
        }

        [TestMethod]
        public void Length_ForZeroVector_ReturnsZero()
        {
            // Arrange
            var vector = new MathVector(0, 0, 0);

            // Act & Assert
            Assert.AreEqual(0, vector.Length, Tolerance);
        }

        [TestMethod]
        public void Length_ForNonZeroVector_ReturnsCorrectValue()
        {
            // Arrange
            var vector = new MathVector(3, 4);

            // Act & Assert
            Assert.AreEqual(5, vector.Length, Tolerance);
        }

        [TestMethod]
        public void Length_ForNegativeComponents_ReturnsCorrectValue()
        {
            // Arrange
            var vector = new MathVector(-3, -4);

            // Act & Assert
            Assert.AreEqual(5, vector.Length, Tolerance);
        }

        [TestMethod]
        public void Indexer_GetAndSet_WorksCorrectly()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);

            // Act & Assert - Get
            Assert.AreEqual(1, vector[0]);
            Assert.AreEqual(2, vector[1]);
            Assert.AreEqual(3, vector[2]);

            // Act & Assert - Set
            vector[1] = 5;
            Assert.AreEqual(5, vector[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Indexer_GetOutOfRange_ThrowsException()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);

            // Act & Assert
            _ = vector[5];
        }
    }
}
