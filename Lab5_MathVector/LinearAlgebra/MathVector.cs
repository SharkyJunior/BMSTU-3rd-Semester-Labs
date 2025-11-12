using System;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MathVectorNS
{
    public class MathVector : IMathVector
    {
        private double[] arr;
        public MathVector(params double[] data)
        {
            if (data == null)
                ArgumentNullException.ThrowIfNull(data);
            arr = new double[data.Length];
            Array.Copy(data, arr, data.Length);
        }

        public MathVector(int size)
        {
            if (size < 0)
                ArgumentOutOfRangeException.ThrowIfNegative(size);
            arr = new double[size];
        }

        public IEnumerator GetEnumerator()
        {
            return arr.GetEnumerator();
        }

        public int Dimensions
        {
            get { return arr.Length; }
        }

        public double this[int i]
        {
            get
            {
                if (i < 0 || i >= Dimensions)
                    throw new ArgumentOutOfRangeException(nameof(i));
                return arr[i];
            }
            set
            {
                if ((i < 0) || (i >= Dimensions))
                    throw new ArgumentOutOfRangeException(nameof(i));
                arr[i] = value;
            }
        }

        public double Length
        {
            get
            {
                double sumSquare = 0;
                foreach (var item in arr)
                {
                    sumSquare += item * item;
                }
                return Math.Sqrt(sumSquare);
            }
        }

        public IMathVector SumNumber(double number)
        {
            var result = new MathVector(Dimensions);
            for (int i = 0; i < Dimensions; ++i)
            {
                result[i] = this[i] + number;
            }
            return result;
        }

        public IMathVector MultiplyNumber(double number)
        {
            var result = new MathVector(Dimensions);
            for (int i = 0; i < Dimensions; ++i)
            {
                result[i] = this[i] * number;
            }
            return result;
        }

        public IMathVector DivideNumber(double number)
        {
            if (number == 0) throw new DivideByZeroException();
            return MultiplyNumber(1.0 / number);
        }

        public IMathVector Sum(IMathVector vector)
        {
            if (vector.Dimensions != Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            var result = new MathVector(Dimensions);
            for (int i = 0; i < Dimensions; ++i)
            {
                result[i] = this[i] + vector[i];
            }
            return result;
        }

        public IMathVector Multiply(IMathVector vector)
        {
            if (vector.Dimensions != Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            var result = new MathVector(Dimensions);
            for (int i = 0; i < Dimensions; ++i)
            {
                result[i] = this[i] * vector[i];
            }
            return result;

        }

        public IMathVector Divide(IMathVector vector)
        {
            if (vector.Dimensions != Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            var result = new MathVector(Dimensions);
            for (int i = 0; i < Dimensions; ++i)
            {
                result[i] = this[i] / vector[i];
            }
            return result;
        }

        public double ScalarMultiply(IMathVector vector)
        {
            if (vector.Dimensions != Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            double result = 0;
            for (int i = 0; i < Dimensions; ++i)
            {
                result += this[i] * vector[i];
            }
            return result;

        }

        public double CalcDistance(IMathVector vector)
        {
            if (vector.Dimensions != Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            double distance = 0;
            for (int i = 0; i < Dimensions; ++i)
            {
                double diff = Math.Abs(this[i] - vector[i]);
                distance += diff * diff;
            }
            return Math.Sqrt(distance);
        }

        public static MathVector operator +(MathVector vector1, MathVector vector2)
        {
            if (vector1.Dimensions != vector2.Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            return (MathVector)vector1.Sum(vector2);
        }

        public static MathVector operator +(MathVector vector, double number)
        {
            return (MathVector)vector.SumNumber(number);
        }

        public static MathVector operator -(MathVector vector1, MathVector vector2)
        {
            if (vector1.Dimensions != vector2.Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            var result = new MathVector(vector1.Dimensions);
            for (int i = 0; i < vector1.Dimensions; ++i)
                result[i] = vector1[i] - vector2[i];
            return result;

        }

        public static MathVector operator -(MathVector vector, double number)
        {
            return (MathVector)vector.SumNumber(-number);
        }

        public static MathVector operator *(MathVector vector1, MathVector vector2)
        {
            if (vector1.Dimensions != vector2.Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            return (MathVector)vector1.Multiply(vector2);
        }

        public static MathVector operator *(MathVector vector, double number)
        {
            return (MathVector)vector.MultiplyNumber(number);
        }

        public static MathVector operator /(MathVector vector1, MathVector vector2)
        {
            if (vector1.Dimensions != vector2.Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            return (MathVector) vector1.Divide(vector2);
        }

        public static MathVector operator /(MathVector vector, double number)
        {
            if (number == 0)
                throw new DivideByZeroException();
            return (MathVector)vector.DivideNumber(number);
        }

        public static double operator %(MathVector vector1, MathVector vector2)
        {
            if (vector1.Dimensions != vector2.Dimensions)
                throw new ArgumentException("Vectors must have the same dimensions");

            return vector1.ScalarMultiply(vector2);
        }

        public override string ToString()
        {
            return $"({string.Join(", ", arr)})";
        }

    }
}


