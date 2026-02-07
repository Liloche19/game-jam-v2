using Godot;
using System;
using FractalGenerator.Math;

namespace FractalGenerator.Fractals
{
	/// <summary>
	/// Base class for all fractal types.
	/// Defines the interface that all fractals must implement.
	/// </summary>
	public abstract class FractalBase
	{
		public enum FractalType
		{
		Julia
	}

	// Core fractal parameters
	public int MaxIterations { get; set; } = 200;
	public float ColorRange { get; set; } = 1f;
	public float ColorShift { get; set; } = 0f;
	public bool SmoothColoring { get; set; } = true;
	public bool DivisionByZeroOccurred { get; set; } = false;
		public Complex CenterPosition { get; set; }
		public float ZoomLevel { get; set; } = 3f; // Width of the viewport in complex plane
		public Complex ViewportSize { get; set; } = new Complex(800, 600);

		public abstract FractalType Type { get; }

		/// <summary>
		/// Escape-time algorithm core function.
		/// Maps a pixel coordinate to a fractal iteration count.
		/// </summary>
		public abstract int ComputeIterations(Complex pixel, out float smoothValue);

		/// <summary>
		/// Resets the fractal to default parameters.
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// Computes the color for a given iteration count using the active palette.
		/// </summary>
		protected Color GetColorFromIterations(int iterations, float smoothValue)
		{
			if (iterations == MaxIterations)
				return new Color(0f, 0f, 0f, 1f); // Black for in-set points

			// Simple smooth coloring: blend based on fractional part
			float smoothIterations = iterations + 1f - MathF.Log(MathF.Log(smoothValue)) / MathF.Log(2f);
			smoothIterations = MathF.Max(0f, smoothIterations);

			float normalized = (smoothIterations / MaxIterations) % 1f;

			// Simple HSV to RGB conversion for smooth coloring
			float h = (normalized + ColorShift) % 1f;
			float s = 1f;
			float v = 1f;

			return Color.FromHsv(h, s, v, 1f);
		}

		/// <summary>
		/// Converts a screen pixel coordinate to a complex number based on current view.
		/// </summary>
		public Complex PixelToComplex(float pixelX, float pixelY)
		{
			float width = ZoomLevel;
			float height = ZoomLevel * ((float)ViewportSize.Imaginary / (float)ViewportSize.Real);

			float real = (pixelX / (float)ViewportSize.Real) * width + CenterPosition.Real - width / 2f;
			float imaginary = ((float)ViewportSize.Imaginary - pixelY) / (float)ViewportSize.Imaginary * height + CenterPosition.Imaginary - height / 2f;

			return new Complex(real, imaginary);
		}

		/// <summary>
		/// Converts a complex number to screen pixel coordinates.
		/// </summary>
		public (float pixelX, float pixelY) ComplexToPixel(Complex complexNum)
		{
			float width = ZoomLevel;
			float height = ZoomLevel * ((float)ViewportSize.Imaginary / (float)ViewportSize.Real);

			float pixelX = ((complexNum.Real - CenterPosition.Real + width / 2f) / width) * (float)ViewportSize.Real;
			float pixelY = ((float)ViewportSize.Imaginary - (complexNum.Imaginary - CenterPosition.Imaginary + height / 2f) / height * (float)ViewportSize.Imaginary);

			return (pixelX, pixelY);
		}
	}
}
