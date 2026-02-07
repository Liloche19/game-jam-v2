using Godot;
using FractalGenerator.Fractals;
using FractalGenerator.UI;

namespace FractalGenerator
{
	/// <summary>
	/// Main scene controller for the Fractal Generator.
	/// Initializes the fractal renderer and UI system.
	/// Handles keyboard input for zoom, pan, and fractal switching.
	/// </summary>
	public partial class FractalScene : Node3D
	{
		private FractalRenderer _fractalRenderer;
		private FractalUI _fractalUI;
		private Camera3D _camera;

		// Input handling
		private float _panSensitivity = 0.1f;
		private float _zoomSensitivity = 1.2f;

		public override void _Ready()
		{
			// Create camera positioned to view the fractal display on the north wall
			_camera = new Camera3D();
			_camera.Position = new Vector3(0, 4, 2);
			_camera.LookAt(new Vector3(0, 4, -4.5), Vector3.Up); // Look at fractal display
			AddChild(_camera);
			_camera.Current = true;

			// Create fractal renderer
			_fractalRenderer = new FractalRenderer();
			_fractalRenderer.TextureWidth = 1600;
			_fractalRenderer.TextureHeight = 1200;
			_fractalRenderer.UseGPURendering = true;
			AddChild(_fractalRenderer);

			// Create UI
			CanvasLayer uiLayer = new CanvasLayer();
			AddChild(uiLayer);

			_fractalUI = new FractalUI();
			_fractalUI.AnchorLeft = 0f;
			_fractalUI.AnchorTop = 0f;
			_fractalUI.AnchorRight = 0f;
			_fractalUI.AnchorBottom = 1f;
			uiLayer.AddChild(_fractalUI);

			GD.Print("Fractal Generator initialized. Use UI to adjust parameters.");
			GD.Print("Controls: WASD for pan, Q/E for zoom, R to reset");
		}

		public override void _Process(double delta)
		{
			HandleInput();
			CheckWinCondition();
		}

		/// <summary>
		/// Checks if the player has found a division by 0 and displays win message.
		/// </summary>
		private void CheckWinCondition()
		{
			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal != null && fractal.DivisionByZeroOccurred)
			{
				GD.Print("🎉 PLAYER WON! Division by Zero Found!");
				// Reset for next try
				fractal.DivisionByZeroOccurred = false;
			}
		}

		/// <summary>
		/// Handles keyboard input for interactive fractal exploration.
		/// </summary>
		private void HandleInput()
		{
			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			// Zoom controls
			if (Input.IsActionPressed("ui_select"))
			{
				_fractalRenderer.ZoomIn(fractal.CenterPosition, _zoomSensitivity);
			}
			if (Input.IsActionPressed("ui_cancel"))
			{
				_fractalRenderer.ZoomOut(_zoomSensitivity);
			}

			// Pan controls (arrow keys or WASD)
			float panX = 0f;
			float panY = 0f;

			if (Input.IsActionPressed("ui_right"))
				panX += _panSensitivity * fractal.ZoomLevel;
			if (Input.IsActionPressed("ui_left"))
				panX -= _panSensitivity * fractal.ZoomLevel;
			if (Input.IsActionPressed("ui_down"))
				panY -= _panSensitivity * fractal.ZoomLevel;
			if (Input.IsActionPressed("ui_up"))
				panY += _panSensitivity * fractal.ZoomLevel;

			if (panX != 0f || panY != 0f)
			{
				_fractalRenderer.Pan(new Math.Complex(panX, panY));
			}

			// Reset view
			if (Input.IsActionJustPressed("ui_home"))
			{
				fractal.Reset();
				_fractalRenderer.MarkForUpdate();
			}
		}
	}
}
