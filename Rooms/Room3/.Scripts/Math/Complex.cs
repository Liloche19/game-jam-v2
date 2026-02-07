using System;

namespace FractalGenerator.Math
{
	/// <summary>
	/// Represents a complex number with real and imaginary parts.
	/// Used for all fractal computations.
	/// </summary>
	public struct Complex
	{
		public float Real;
		public float Imaginary;

		public Complex(float real = 0f, float imaginary = 0f)
		{
			Real = real;
			Imaginary = imaginary;
		}

		/// <summary>
		/// Gets the squared modulus (magnitude squared) of the complex number.
		/// Used for performance optimization in escape-time fractals.
		/// </summary>
		public float ModulusSquared => Real * Real + Imaginary * Imaginary;

		/// <summary>
		/// Gets the modulus (magnitude) of the complex number.
		/// </summary>
		public float Modulus => MathF.Sqrt(ModulusSquared);

		/// <summary>
		/// Gets the argument (angle) of the complex number in radians.
		/// </summary>
		public float Argument
		{
			get
			{
				if (Modulus == 0f)
					return 0f;

				if (Imaginary >= 0f)
					return MathF.Acos(Real / Modulus);
				else
					return 2f * MathF.PI - MathF.Acos(Real / Modulus);
			}
		}

		/// <summary>
		/// Adds two complex numbers: (a + bi) + (c + di) = (a+c) + (b+d)i
		/// </summary>
		public static Complex operator +(Complex a, Complex b)
		{
			return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
		}

		/// <summary>
		/// Subtracts two complex numbers: (a + bi) - (c + di) = (a-c) + (b-d)i
		/// </summary>
		public static Complex operator -(Complex a, Complex b)
		{
			return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
		}

		/// <summary>
		/// Multiplies two complex numbers: (a + bi) * (c + di) = (ac - bd) + (ad + bc)i
		/// </summary>
		public static Complex operator *(Complex a, Complex b)
		{
			return new Complex(
				a.Real * b.Real - a.Imaginary * b.Imaginary,
				a.Real * b.Imaginary + a.Imaginary * b.Real
			);
		}

		/// <summary>
		/// Multiplies a complex number by a scalar.
		/// </summary>
		public static Complex operator *(Complex a, float scalar)
		{
			return new Complex(a.Real * scalar, a.Imaginary * scalar);
		}

		/// <summary>
		/// Divides two complex numbers: (a + bi) / (c + di) = [(ac + bd) + (bc - ad)i] / (c² + d²)
		/// Prevents division by zero by returning zero for zero divisor.
		/// </summary>
		public static Complex operator /(Complex a, Complex b)
		{
			float modulusSquared = b.ModulusSquared;

			if (modulusSquared == 0f)
				return new Complex(0f, 0f);

			return new Complex(
				(a.Real * b.Real + a.Imaginary * b.Imaginary) / modulusSquared,
				(a.Imaginary * b.Real - a.Real * b.Imaginary) / modulusSquared
			);
		}

		/// <summary>
		/// Divides a complex number by a scalar.
		/// </summary>
		public static Complex operator /(Complex a, float scalar)
		{
			if (scalar == 0f)
				return new Complex(0f, 0f);

			return new Complex(a.Real / scalar, a.Imaginary / scalar);
		}

		/// <summary>
		/// Squares a complex number: (a + bi)² = (a² - b²) + (2ab)i
		/// </summary>
		public Complex Square()
		{
			return new Complex(
				Real * Real - Imaginary * Imaginary,
				2f * Real * Imaginary
			);
		}

		/// <summary>
		/// Computes z^n for integer n.
		/// </summary>
		public Complex Power(int n)
		{
			if (n == 0)
				return new Complex(1f, 0f);
			if (n == 1)
				return this;
			if (n == 2)
				return Square();

			Complex result = this;
			for (int i = 1; i < n; i++)
				result *= this;

			return result;
		}

		public override string ToString()
		{
			if (Imaginary >= 0)
				return $"{Real} + {Imaginary}i";
			else
				return $"{Real} - {-Imaginary}i";
		}
	}
}
