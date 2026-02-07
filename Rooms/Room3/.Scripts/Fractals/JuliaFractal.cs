using System;
using Godot;
using FractalGenerator.Math;

namespace FractalGenerator.Fractals
{
	/// <summary>
	/// Rational Julia fractal generator.
	/// Uses the formula: z = 1 / (z² - shift)
	/// Division by 0 occurs when z² = shift, creating a win condition.
	/// Different shift values produce different fractal structures.
	/// 
	/// Mathematical formula: z_{n+1} = 1 / (z_n^2 - shift)
	/// When z² equals shift, division by 0 triggers the win condition!
	/// </summary>
	public class JuliaFractal : FractalBase
	{
		public override FractalType Type => FractalType.Julia;

		/// <summary>
		/// The shift parameter that defines where division by 0 can happen.
		/// When z² = shift, we get division by 0 (the win condition!).
		/// </summary>
		public Complex ShiftParameter { get; set; } = new Complex(0.25f, 0.5f); // Non-trivial starting point
		public int PaletteIndex { get; set; } = 0;

		public JuliaFractal()
		{
			Reset();
		}

		public override void Reset()
		{
			CenterPosition = new Complex(0f, 0f);
			ZoomLevel = 3f;
			MaxIterations = 100;
			ColorRange = 1f;
			ColorShift = 0f;
			SmoothColoring = true;
			ShiftParameter = new Complex(0.25f, 0.5f); // Non-trivial starting point
			DivisionByZeroOccurred = false;
		}

		/// <summary>
		/// Computes rational Julia set iterations using z = 1 / (z² - shift).
		/// Division by 0 occurs when z² = shift, which is the WIN CONDITION!
		/// This is the game mechanic: find parameters that cause division by 0.
		/// </summary>
		public override int ComputeIterations(Complex z, out float smoothValue)
		{
			int iterations = 0;
			float maxModulusSq = 100f * 100f; // Larger escape radius for rational function
			smoothValue = 0f;
			float divisionByZeroThreshold = 1e-6f; // Very small threshold to detect near-division

			if (SmoothColoring)
				smoothValue = MathF.Exp(-z.Modulus);

			while (z.ModulusSquared < maxModulusSq && iterations < MaxIterations)
			{
				// Compute z² - shift (the denominator)
				Complex denominator = z.Square() - ShiftParameter;
				float denominatorModulus = denominator.Modulus;

				// Check for division by 0 (WIN CONDITION!)
				if (denominatorModulus < divisionByZeroThreshold)
				{
					DivisionByZeroOccurred = true;
					smoothValue = float.PositiveInfinity; // Mark as special
					return iterations; // Return early - we found the win!
				}

				// z = 1 / (z² - shift)
				z = new Complex(1f, 0f) / denominator;
				iterations++;

				if (SmoothColoring)
					smoothValue += MathF.Exp(-z.Modulus);
			}

			// Compute smooth iteration value
			if (iterations < MaxIterations && SmoothColoring)
			{
				float modulus = z.Modulus;
				if (modulus > 0 && modulus < float.PositiveInfinity)
					smoothValue = iterations + 1f - MathF.Log(MathF.Log(modulus)) / MathF.Log(2f);
				else
					smoothValue = iterations + 1f;
			}
			else if (!SmoothColoring)
			{
				smoothValue = iterations;
			}

			return iterations;
		}

		/// <summary>
		/// Sets the shift parameter from two float components.
		/// Adjusting these values changes where division by 0 can occur!
		/// </summary>
		public void SetShiftParameter(float real, float imaginary)
		{
			ShiftParameter = new Complex(real, imaginary);
		}
	}
}
