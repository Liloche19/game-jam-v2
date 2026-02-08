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
		private bool _winTriggered = false;

		// Input handling
		private float _panSensitivity = 0.1f;
		private float _zoomSensitivity = 1.2f;

		public override void _Ready()
		{
			try
			{
				GD.Print("FractalScene: Starting initialization...");
				EnsureFractalInputBindings();
				
				// Find the Player's Camera3D by searching the scene tree
				_camera = GetTree().CurrentScene?.FindChild("Camera3D", true, false) as Camera3D;
				if (_camera == null)
				{
					GD.PrintErr("FractalScene: Could not find Camera3D in scene!");
				}
				else
				{
					GD.Print("FractalScene: Found camera");
				}

			// Create fractal renderer
			GD.Print("FractalScene: Creating FractalRenderer...");
			_fractalRenderer = new FractalRenderer();
			_fractalRenderer.Name = "FractalRenderer";
			_fractalRenderer.TextureWidth = 1600;
			_fractalRenderer.TextureHeight = 1200;
			// Prefer GPU rendering for responsiveness.
			_fractalRenderer.UseGPURendering = false;
			AddChild(_fractalRenderer);
			GD.Print("FractalScene: FractalRenderer created");
			
			// Create UI Layer
			GD.Print("FractalScene: Creating UI layer...");
			CanvasLayer uiLayer = new CanvasLayer();
			uiLayer.Layer = 1;
			AddChild(uiLayer);
			
			// Create and configure UI
			GD.Print("FractalScene: Creating FractalUI...");
			_fractalUI = new FractalUI();
			_fractalUI.AnchorLeft = 0f;
			_fractalUI.AnchorTop = 0f;
			_fractalUI.AnchorRight = 0.3f;
			_fractalUI.AnchorBottom = 1f;
			_fractalUI.CustomMinimumSize = new Vector2(400, 0);
			uiLayer.AddChild(_fractalUI);
			// Bind UI to the renderer so controls affect the correct instance
			_fractalUI.BindRenderer(_fractalRenderer);
			GD.Print("FractalScene: FractalUI created");
			
			// Show cursor for UI interaction in this room
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GD.Print("FractalScene: Cursor enabled");
			
			GD.Print("FractalScene initialization complete!");
			}
			catch (Exception e)
			{
				GD.PrintErr($"FractalScene._Ready exception: {e}");
				GD.PrintErr($"Stack trace: {e.StackTrace}");
			}
		}

		public override void _ExitTree()
		{
			// Hide cursor when leaving this room
			Input.MouseMode = Input.MouseModeEnum.Captured;
			GD.Print("FractalScene: Cursor disabled");
		}

		public override void _Process(double delta)
		{
			// Handle input and win condition check
			HandleInput();
			CheckWinCondition();
		}

		/// <summary>
		/// Checks if the player has found a division by 0 and displays win message.
		/// </summary>
		private void CheckWinCondition()
		{
			if (_fractalRenderer == null) return;
			
			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
		if (fractal == null) return;
		
		// For GPU rendering: Check if v parameter is in a range that causes visible division by zero
		// For CPU rendering: Check the DivisionByZeroOccurred flag
		bool divisionDetected = false;
		
		// Use CPU-detected flag for win condition (works for both CPU and GPU rendering when CPU verifies)
		divisionDetected = fractal.DivisionByZeroOccurred;
		
		if (divisionDetected && !_winTriggered)
		{
			_winTriggered = true;
			GD.Print("🎉 DIVISION BY ZERO ACHIEVED! YOU WIN! 🎉");
			DisableFloorCollisions();
			// Reset the flag so we don't keep triggering
			fractal.DivisionByZeroOccurred = false;
		}
	}

	/// <summary>
	/// Handles keyboard input for interactive fractal exploration.
	/// </summary>
	private void HandleInput()
		{
			if (_fractalRenderer == null) return;
			
			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			// Zoom controls (use custom actions to avoid jump key conflicts)
			if (Input.IsActionPressed("fractal_zoom_in"))
			{
				_fractalRenderer.ZoomIn(fractal.CenterPosition, _zoomSensitivity);
			}
			if (Input.IsActionPressed("fractal_zoom_out"))
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

			if (Input.IsActionJustPressed("fractal_render_cpu"))
			{
				_fractalUI?.RequestCpuRenderFromKey();
			}
		}

		private void EnsureFractalInputBindings()
		{
			EnsureActionBinding("fractal_zoom_in", Key.Pageup);
			EnsureActionBinding("fractal_zoom_out", Key.Pagedown);
			EnsureActionBinding("fractal_render_cpu", Key.R);
		}

		private void EnsureActionBinding(string actionName, Key key)
		{
			if (InputMap.HasAction(actionName))
				return;
			InputMap.AddAction(actionName);
			InputMap.ActionAddEvent(actionName, new InputEventKey { Keycode = key });
		}

		private void DisableFloorCollisions()
		{
			var floor = GetTree().CurrentScene?.FindChild("Floor", true, false) as StaticBody3D;
			if (floor == null)
			{
				GD.PrintErr("FractalScene: Floor not found; cannot disable collisions");
				return;
			}

			floor.CollisionLayer = 0;
			floor.CollisionMask = 0;
			foreach (Node child in floor.GetChildren())
			{
				if (child is CollisionShape3D shape)
					shape.Disabled = true;
			}
		}
	}
}
