using System;
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
			try
			{
				GD.Print("FractalScene: Starting initialization...");
				
				// Find existing camera from Player node (don't create a new one)
				_camera = GetNodeOrNull<Camera3D>("Player/Camera3D");
				if (_camera == null)
				{
					GD.PrintErr("FractalScene: Could not find Player Camera3D!");
				}
				else
				{
					GD.Print("FractalScene: Found player camera");
				}

				// TEMPORARILY DISABLED - Fractal rendering
				/*
				// Create fractal renderer
				GD.Print("FractalScene: Creating FractalRenderer...");
				_fractalRenderer = new FractalRenderer();
				_fractalRenderer.Name = "FractalRenderer"; // Set name so UI can find it
				_fractalRenderer.TextureWidth = 1600;
				_fractalRenderer.TextureHeight = 1200;
				_fractalRenderer.UseGPURendering = true;
				AddChild(_fractalRenderer);
				GD.Print("FractalScene: FractalRenderer created");

				// Create UI Layer
				GD.Print("FractalScene: Creating UI layer...");
				CanvasLayer uiLayer = new CanvasLayer();
				uiLayer.Layer = 1; // Render on top
				AddChild(uiLayer);

				// Create and configure UI
				GD.Print("FractalScene: Creating FractalUI...");
				_fractalUI = new FractalUI();
				_fractalUI.AnchorLeft = 0f;
				_fractalUI.AnchorTop = 0f;
				_fractalUI.AnchorRight = 0f;
				_fractalUI.AnchorBottom = 1f;
				_fractalUI.CustomMinimumSize = new Vector2(400, 0); // Set width for left panel
				uiLayer.AddChild(_fractalUI);
				GD.Print("FractalScene: FractalUI created");
				*/

				GD.Print("FractalScene initialization complete (fractal rendering disabled for testing).");
			}
			catch (Exception e)
			{
				GD.PrintErr($"FractalScene._Ready exception: {e}");
				GD.PrintErr($"Stack trace: {e.StackTrace}");
			}
		}

		public override void _Process(double delta)
		{
			// TEMPORARILY DISABLED for testing
			// HandleInput();
			// CheckWinCondition();
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
