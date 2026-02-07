using Godot;
using System;
using FractalGenerator.Fractals;
using FractalGenerator.Math;

namespace FractalGenerator
{
	/// <summary>
	/// Handles rendering fractals to a texture.
	/// Supports both CPU-based computation and GPU-based computation via shaders.
	/// 
	/// For best performance:
	/// - Use GPU rendering when available (via fragment shaders)
	/// - Fall back to CPU rendering for slower machines or testing
	/// - Render at lower resolution and upscale for real-time performance
	/// </summary>
	public partial class FractalRenderer : Node
	{
		[Export] public int TextureWidth { get; set; } = 800;
		[Export] public int TextureHeight { get; set; } = 600;
		[Export] public bool UseGPURendering { get; set; } = true;

		private FractalBase _currentFractal;
		private Image _fractalImage;
		private ImageTexture _fractalTexture;
		private MeshInstance3D _displayMesh;
		private bool _needsUpdate = true;

		// GPU shader resources
		private ShaderMaterial _cachedShaderMaterial;
		private BaseMaterial3D _cachedCPUMaterial;

		public override void _Ready()
		{
		// Find the FractalDisplay mesh in the scene (should be in room.tscn)
		_displayMesh = GetParent().FindChild("FractalDisplay", true, false) as MeshInstance3D;
		
		if (_displayMesh == null)
		{
			// Fallback: Create display mesh if not found in scene
			GD.PrintErr("FractalRenderer: FractalDisplay not found in scene, creating new mesh");
			_displayMesh = new MeshInstance3D();
			AddChild(_displayMesh);

			// Create a plane mesh to display the fractal
			PlaneMesh planeMesh = new PlaneMesh();
			planeMesh.Size = new Vector2(10, 7.5f); // 4:3 aspect ratio
			_displayMesh.Mesh = planeMesh;
		}
			StandardMaterial3D material = new StandardMaterial3D();
			material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
			_displayMesh.SetSurfaceOverrideMaterial(0, material);

			// Initialize with Julia fractal (only fractal type now)
			_currentFractal = new JuliaFractal();
			_currentFractal.ViewportSize = new Complex(TextureWidth, TextureHeight);

			// Create initial texture (used for CPU rendering fallback)
			_fractalImage = Image.CreateEmpty(TextureWidth, TextureHeight, false, Image.Format.Rgba8);
			_fractalTexture = ImageTexture.CreateFromImage(_fractalImage);

			// Apply texture to mesh material as fallback
			material.AlbedoTexture = _fractalTexture;

			MarkForUpdate();
		}

		/// <summary>
		/// Marks the fractal for update on the next frame.
		/// </summary>
		public void MarkForUpdate()
		{
			_needsUpdate = true;
		}

		/// <summary>
		/// Sets the active fractal to render.
		/// </summary>
		public void SetFractal(FractalBase fractal)
		{
			_currentFractal = fractal;
			_currentFractal.ViewportSize = new Complex(TextureWidth, TextureHeight);
			MarkForUpdate();
		}

		public override void _Process(double delta)
		{
			if (_needsUpdate && _currentFractal != null)
			{
				if (UseGPURendering)
					RenderFractalGPU();
				else
					RenderFractalCPU();

				_needsUpdate = false;
			}
		}

		/// <summary>
		/// CPU-based fractal rendering.
		/// Iterates through each pixel and computes the fractal value.
		/// Slower but always works and is good for debugging.
		/// </summary>
		private void RenderFractalCPU()
		{
			for (int y = 0; y < TextureHeight; y++)
			{
				for (int x = 0; x < TextureWidth; x++)
				{
					// Convert pixel to complex coordinate
					Complex pixelComplex = _currentFractal.PixelToComplex(x, y);

					// Compute iteration count
					int iterations = _currentFractal.ComputeIterations(pixelComplex, out float smoothValue);

					// Get color from iterations
					Color pixelColor = GetColorFromIterations(iterations, smoothValue);

					_fractalImage.SetPixel(x, y, pixelColor);
				}
			}

			_fractalTexture.Update(_fractalImage);
		}

		/// <summary>
		/// GPU-based fractal rendering using Godot shaders.
		/// Much faster for real-time interactive zoom and pan.
		/// </summary>
		private void RenderFractalGPU()
		{
			// Initialize shader material on first GPU render
			if (_cachedShaderMaterial == null)
			{
				_cachedShaderMaterial = new ShaderMaterial();
				Shader shader = GD.Load<Shader>("res://Shaders/fractal.gdshader");
				
				if (shader == null)
				{
					GD.PrintErr("Failed to load fractal shader at res://Shaders/fractal.gdshader. Falling back to CPU rendering.");
					UseGPURendering = false;
					RenderFractalCPU();
					return;
				}

				_cachedShaderMaterial.Shader = shader;
				GD.Print("Shader loaded successfully. Using GPU rendering.");
			}

			// Update shader parameters each frame
			_cachedShaderMaterial.SetShaderParameter("center_x", _currentFractal.CenterPosition.Real);
			_cachedShaderMaterial.SetShaderParameter("center_y", _currentFractal.CenterPosition.Imaginary);
			_cachedShaderMaterial.SetShaderParameter("zoom", _currentFractal.ZoomLevel);
			_cachedShaderMaterial.SetShaderParameter("max_iterations", _currentFractal.MaxIterations);
			_cachedShaderMaterial.SetShaderParameter("color_range", _currentFractal.ColorRange);
			_cachedShaderMaterial.SetShaderParameter("color_shift", _currentFractal.ColorShift);
			_cachedShaderMaterial.SetShaderParameter("fractal_type", (int)_currentFractal.Type);

			// For Julia set, pass the Julia constant
			if (_currentFractal is JuliaFractal julia)
			{
				_cachedShaderMaterial.SetShaderParameter("julia_c_real", julia.JuliaConstant.Real);
				_cachedShaderMaterial.SetShaderParameter("julia_c_imag", julia.JuliaConstant.Imaginary);
			}

			// Apply shader material to mesh
			_displayMesh.SetSurfaceOverrideMaterial(0, _cachedShaderMaterial);
		}

		/// <summary>
		/// Converts iteration count to a color using smooth coloring.
		/// </summary>
		private Color GetColorFromIterations(int iterations, float smoothValue)
		{
			if (iterations == _currentFractal.MaxIterations)
				return new Color(0f, 0f, 0f, 1f); // Black for in-set

			// Normalize iteration count
			float normalized = (smoothValue / _currentFractal.MaxIterations) % 1f;

			// Apply color shift
			float hue = (normalized + _currentFractal.ColorShift) % 1f;

			// Convert HSV to RGB for smooth coloring
			return Color.FromHsv(hue, 1f, 1f, 1f);
		}

		/// <summary>
		/// Zoom in on a specific point in the complex plane.
		/// </summary>
		public void ZoomIn(Complex point, float factor = 2f)
		{
			_currentFractal.CenterPosition = point;
			_currentFractal.ZoomLevel /= factor;
			MarkForUpdate();
		}

		/// <summary>
		/// Zoom out from a point.
		/// </summary>
		public void ZoomOut(float factor = 2f)
		{
			_currentFractal.ZoomLevel *= factor;
			MarkForUpdate();
		}

		/// <summary>
		/// Pan the view by a complex offset.
		/// </summary>
		public void Pan(Complex offset)
		{
			_currentFractal.CenterPosition += offset;
			MarkForUpdate();
		}

		/// <summary>
		/// Gets the current fractal being rendered.
		/// </summary>
		public FractalBase GetCurrentFractal() => _currentFractal;
	}
}
