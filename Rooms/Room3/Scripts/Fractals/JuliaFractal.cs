using System;
using Godot;
using FractalGenerator.Math;

namespace FractalGenerator.Fractals
{
	/// <summary>
	/// Rational Julia fractal generator.
	/// Uses the formula: z = (k / (z - v))^2 + x
	/// Division by 0 occurs when z - v = 0, creating a win condition.
	/// Different v values control WHERE the singularity occurs.
	/// 
	/// Mathematical formula: z_{n+1} = (k / (z_n - v))^2 + x
	/// When z - v equals 0, division by 0 triggers the win condition!
	/// </summary>
	public class JuliaFractal : FractalBase
	{
		public override FractalType Type => FractalType.Julia;

		/// <summary>
		/// The v parameter that the player can modify - this is the SINGULARITY point!
		/// Division by zero occurs when z equals v.
		/// </summary>
		public Complex VParameter { get; set; } = new Complex(0.25f, 0.5f); // Non-trivial starting point
		/// <summary>
		/// Constant x in the formula (not player-modifiable) - the Julia constant.
		/// </summary>
		public Complex ConstantX { get; } = new Complex(0.15f, -0.2f);
		/// <summary>
		/// Constant k in the formula (real scalar).
		/// </summary>
		public float ConstantK { get; } = 1.0f;
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
			VParameter = new Complex(0.25f, 0.5f); // Non-trivial starting point
			DivisionByZeroOccurred = false;
		}

		/// <summary>
		/// Computes rational Julia set iterations using z = (k / (z - v))^2 + x.
		/// Division by 0 occurs when z - v = 0, which is the WIN CONDITION!
		/// This is the game mechanic: find v that causes division by 0.
		/// </summary>
		public override int ComputeIterations(Complex z, out float smoothValue)
		{
			int iterations = 0;
			float maxModulusSq = 100f * 100f; // Larger escape radius for rational function
			smoothValue = 0f;
			float divisionByZeroThreshold = 1e-6f;
			Complex k = new Complex(ConstantK, 0f);

			if (SmoothColoring)
				smoothValue = MathF.Exp(-z.Modulus);

			while (z.ModulusSquared < maxModulusSq && iterations < MaxIterations)
			{
				// Compute z - v (the denominator - v is the player-controlled singularity!)
				Complex denominator = z - VParameter;
				float denominatorModulus = denominator.Modulus;

				// Check for division by 0 (WIN CONDITION!)
				if (denominatorModulus < divisionByZeroThreshold)
				{
					DivisionByZeroOccurred = true;
					smoothValue = float.PositiveInfinity; // Mark as special
					return iterations; // Return early - we found the win!
				}

				// z = (k / (z - v))^2 + x
				z = (k / denominator).Square() + ConstantX;
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
		/// Sets the v parameter from two float components.
		/// Adjusting these values changes where division by 0 can occur.
		/// </summary>
		public void SetVParameter(float real, float imaginary)
		{
			VParameter = new Complex(real, imaginary);
		}
	}
}
