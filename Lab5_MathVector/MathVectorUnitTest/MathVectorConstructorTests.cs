using MathVectorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathVectorUnitTest
{
    [TestClass]
    public class MathVectorConstructorTests
    {
        [TestMethod]
        public void Constructor_WithDimension_CreatesVectorWithCorrectSize()
        {
            // Arrange & Act
            var vector = new MathVector(5);

            // Assert
            Assert.AreEqual(5, vector.Dimensions);
        }

        [TestMethod]
        public void Constructor_WithParams_CreatesVectorWithCorrectValues()
        {
            // Arrange & Act
            var vector = new MathVector(1, 2, 3);

            // Assert
            Assert.AreEqual(3, vector.Dimensions);
            Assert.AreEqual(1, vector[0]);
            Assert.AreEqual(2, vector[1]);
            Assert.AreEqual(3, vector[2]);
        }

        [TestMethod]
        public void Constructor_WithParams_AllowsEnumeration()
        {
            // Arrange
            var vector = new MathVector(1, 2, 3);
            var expected = new List<double> { 1, 2, 3 };
            var actual = new List<double>();

            // Act
            foreach (double val in vector)
            {
                actual.Add(val);
            }

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_EmptyParams_CreatesZeroDimensionVector()
        {
            // Arrange & Act
            var vector = new MathVector();

            // Assert
            Assert.AreEqual(0, vector.Dimensions);
        }
    }
}
