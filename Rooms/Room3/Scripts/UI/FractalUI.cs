using Godot;
using System;
using System.Collections.Generic;
using FractalGenerator;
using FractalGenerator.Fractals;

namespace FractalGenerator.UI
{
	/// <summary>
	/// UI controller for the Division by Zero Fractal game.
	/// Provides real-time adjustment of:
	/// - Max iterations
	/// - Zoom level and center position (pan)
	/// - Color range and shift
	/// - Shift Parameters (where division by 0 occurs)
	/// 
	/// GAME OBJECTIVE: Find shift parameters that cause division by 0!
	/// When z² = shift during iteration, the fractal encounters division by 0 - YOU WIN!
	/// </summary>
	public partial class FractalUI : Control
	{
		private FractalRenderer _fractalRenderer;

	// UI Elements - Common Parameters
	private SpinBox _maxIterationsSpinBox;
	private HSlider _colorRangeSlider;
	private HSlider _colorShiftSlider;
	private CheckBox _smoothColoringCheckBox;

	// UI Elements - View Parameters
	private SpinBox _panXSpinBox;
	private SpinBox _panYSpinBox;
	private SpinBox _zoomSpinBox;

	// UI Elements - v Parameters (Julia)
	private SpinBox _vRealSpinBox;
	private SpinBox _vImagSpinBox;
	private Button _renderButton;
	private Label _renderHintLabel;
	private bool _pendingCpuRender = false;
	private bool _updatingUI = false; // Prevent feedback loops



		public override void _Ready()
		{
		try
		{
			GD.Print("FractalUI: Starting _Ready...");
			
			// Allow mouse motion to pass through for camera control
			MouseFilter = MouseFilterEnum.Pass;
			
			GD.Print("FractalUI: Looking for FractalRenderer...");
			_fractalRenderer = GetTree().CurrentScene?.FindChild("FractalRenderer", true, false) as FractalRenderer;

			GD.Print("FractalUI: Found FractalRenderer (or will wait to be bound), creating UI panel...");

			// Create UI Panel
			PanelContainer mainPanel = new PanelContainer();
			VBoxContainer mainContainer = new VBoxContainer();
			mainPanel.AddChild(mainContainer);
			
			GD.Print("FractalUI: Creating title and labels...");

			// Create title
			Label titleLabel = new Label { Text = "Division by Zero Fractal" };
			titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			titleLabel.LabelSettings = new LabelSettings();
			titleLabel.LabelSettings.FontSize = 24;
			mainContainer.AddChild(titleLabel);

			mainContainer.AddChild(new HSeparator());

			// Common Parameters
			mainContainer.AddChild(new Label { Text = "Parameters:" });
			
			GD.Print("FractalUI: Creating parameter controls...");

			// Max Iterations
			mainContainer.AddChild(new Label { Text = "Max Iterations:" });
			_maxIterationsSpinBox = new SpinBox();
			_maxIterationsSpinBox.MinValue = 10;
			_maxIterationsSpinBox.MaxValue = 5000;
			_maxIterationsSpinBox.ValueChanged += OnParameterChanged;
			mainContainer.AddChild(_maxIterationsSpinBox);

			// Color Range
			mainContainer.AddChild(new Label { Text = "Color Range:" });
			_colorRangeSlider = new HSlider();
			_colorRangeSlider.MinValue = 0.01f;
			_colorRangeSlider.MaxValue = 10f;
			_colorRangeSlider.ValueChanged += OnParameterChanged;
			mainContainer.AddChild(_colorRangeSlider);

			// Color Shift
			mainContainer.AddChild(new Label { Text = "Color Shift:" });
			_colorShiftSlider = new HSlider();
			_colorShiftSlider.MinValue = 0f;
			_colorShiftSlider.MaxValue = 1f;
			_colorShiftSlider.ValueChanged += OnParameterChanged;
			mainContainer.AddChild(_colorShiftSlider);

			// Smooth Coloring
			_smoothColoringCheckBox = new CheckBox { Text = "Smooth Coloring" };
			_smoothColoringCheckBox.Pressed += () => OnParameterChanged();
			mainContainer.AddChild(_smoothColoringCheckBox);

			mainContainer.AddChild(new HSeparator());

			// View Parameters
			mainContainer.AddChild(new Label { Text = "View:" });

			// Pan X
			mainContainer.AddChild(new Label { Text = "Center X:" });
			_panXSpinBox = new SpinBox();
			_panXSpinBox.MinValue = -10f;
			_panXSpinBox.MaxValue = 10f;
			_panXSpinBox.Step = 0.01f;
			_panXSpinBox.ValueChanged += OnViewChanged;
			mainContainer.AddChild(_panXSpinBox);

			// Pan Y
			mainContainer.AddChild(new Label { Text = "Center Y:" });
			_panYSpinBox = new SpinBox();
			_panYSpinBox.MinValue = -10f;
			_panYSpinBox.MaxValue = 10f;
			_panYSpinBox.Step = 0.01f;
			_panYSpinBox.ValueChanged += OnViewChanged;
			mainContainer.AddChild(_panYSpinBox);

			// Zoom
			mainContainer.AddChild(new Label { Text = "Zoom:" });
			_zoomSpinBox = new SpinBox();
			_zoomSpinBox.MinValue = 0.1f;
			_zoomSpinBox.MaxValue = 10f;
			_zoomSpinBox.Step = 0.1f;
			_zoomSpinBox.ValueChanged += OnViewChanged;
			mainContainer.AddChild(_zoomSpinBox);

			mainContainer.AddChild(new HSeparator());
			
			GD.Print("FractalUI: Creating v parameter controls...");

			// v Parameters (Division by Zero Parameters)
			mainContainer.AddChild(new Label { Text = "Parameter:" });

			mainContainer.AddChild(new Label { Text = "Real:" });
			_vRealSpinBox = new SpinBox();
			_vRealSpinBox.MinValue = -1.5f;
			_vRealSpinBox.MaxValue = 1.5f;
			_vRealSpinBox.Step = 0.01f;
			_vRealSpinBox.ValueChanged += OnVChanged;
			mainContainer.AddChild(_vRealSpinBox);

			mainContainer.AddChild(new Label { Text = "Imaginary:" });
			_vImagSpinBox = new SpinBox();
			_vImagSpinBox.MinValue = -1.5f;
			_vImagSpinBox.MaxValue = 1.5f;
			_vImagSpinBox.Step = 0.01f;
			_vImagSpinBox.ValueChanged += OnVChanged;
			mainContainer.AddChild(_vImagSpinBox);

			// Reset button
			mainContainer.AddChild(new HSeparator());
			Button resetButton = new Button { Text = "Reset to Default" };
			resetButton.Pressed += OnResetPressed;
			mainContainer.AddChild(resetButton);

			// Render button (CPU mode only)
			_renderButton = new Button { Text = "Render (CPU)" };
			_renderButton.Pressed += OnRenderPressed;
			mainContainer.AddChild(_renderButton);

			_renderHintLabel = new Label { Text = "" };
			mainContainer.AddChild(_renderHintLabel);

			// Add the main panel to this control so it's visible
			AddChild(mainPanel);

			GD.Print("FractalUI: Updating UI from fractal...");
			UpdateUIFromFractal();
			GD.Print("FractalUI: _Ready complete!");
		}
		catch (Exception e)
		{
			GD.PrintErr($"FractalUI._Ready exception: {e}");
			GD.PrintErr($"Stack trace: {e.StackTrace}");
		}
	}

	/// <summary>
	/// Bind a FractalRenderer instance to this UI. Useful when the renderer
	/// is created after the UI or when the automatic search didn't find it.
	/// </summary>
	public void BindRenderer(FractalRenderer renderer)
	{
		_fractalRenderer = renderer;
		GD.Print("FractalUI: Bound to FractalRenderer via BindRenderer()");
		UpdateUIFromFractal();
	}

	private void OnFractalTypeChanged(long index)
	{
			// No longer needed - only Julia fractal exists
			_fractalRenderer.SetFractal(new JuliaFractal());
			UpdateUIFromFractal();
		}

		private void OnParameterChanged(double value = 0)
		{
			if (_updatingUI) return;
			if (_fractalRenderer == null) return;

			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			fractal.MaxIterations = (int)_maxIterationsSpinBox.Value;
			fractal.ColorRange = (float)_colorRangeSlider.Value;
			fractal.ColorShift = (float)_colorShiftSlider.Value;
			fractal.SmoothColoring = _smoothColoringCheckBox.ButtonPressed;

			RequestRenderOrQueue();
		}

		private void OnViewChanged(double value = 0)
		{
			if (_updatingUI) return;
			if (_fractalRenderer == null) return;

			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			fractal.CenterPosition = new Math.Complex((float)_panXSpinBox.Value, (float)_panYSpinBox.Value);
			fractal.ZoomLevel = (float)_zoomSpinBox.Value;

			RequestRenderOrQueue();
		}

		private void OnJuliaChanged(double value = 0)
		{
			if (_updatingUI) return;
			if (_fractalRenderer == null) return;

			if (_fractalRenderer.GetCurrentFractal() is JuliaFractal julia)
			{
				julia.SetVParameter((float)_vRealSpinBox.Value, (float)_vImagSpinBox.Value);
				RequestRenderOrQueue();
			}
		}

		private void OnVChanged(double value = 0)
		{
			if (_updatingUI) return;
			if (_fractalRenderer == null) return;

			if (_fractalRenderer.GetCurrentFractal() is JuliaFractal julia)
			{
				julia.SetVParameter((float)_vRealSpinBox.Value, (float)_vImagSpinBox.Value);
				RequestRenderOrQueue();
			}
		}

		private void OnResetPressed()
		{
			if (_fractalRenderer == null) return;
			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal != null)
			{
				fractal.Reset();
				RequestRenderOrQueue();
				UpdateUIFromFractal();
			}
		}

		private void OnRenderPressed()
		{
			if (_fractalRenderer == null) return;
			TriggerRender();
		}

		public void RequestCpuRenderFromKey()
		{
			if (_fractalRenderer == null) return;
			if (_fractalRenderer.UseGPURendering) return;
			TriggerRender();
		}

		private void TriggerRender()
		{
			_pendingCpuRender = false;
			_fractalRenderer.MarkForUpdate();
			UpdateRenderControls();
		}

		private void RequestRenderOrQueue()
		{
			if (_fractalRenderer == null) return;
			if (_fractalRenderer.UseGPURendering)
			{
				_fractalRenderer.MarkForUpdate();
				_pendingCpuRender = false;
			}
			else
			{
				_pendingCpuRender = true;
			}
			UpdateRenderControls();
		}

		/// <summary>
		/// Updates UI elements to reflect current fractal state.
		/// </summary>
		private void UpdateUIFromFractal()
		{
			_updatingUI = true;
			if (_fractalRenderer == null)
			{
				_updatingUI = false;
				return;
			}

			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			// Update common parameters
			_maxIterationsSpinBox.Value = fractal.MaxIterations;
			_colorRangeSlider.Value = fractal.ColorRange;
			_colorShiftSlider.Value = fractal.ColorShift;
			_smoothColoringCheckBox.ButtonPressed = fractal.SmoothColoring;

			// Update view parameters
			_panXSpinBox.Value = fractal.CenterPosition.Real;
			_panYSpinBox.Value = fractal.CenterPosition.Imaginary;
			_zoomSpinBox.Value = fractal.ZoomLevel;

			// Update v parameters (Julia only - always visible now)
			if (fractal is JuliaFractal julia)
			{
				_vRealSpinBox.Value = julia.VParameter.Real;
				_vImagSpinBox.Value = julia.VParameter.Imaginary;
			}

			UpdateRenderControls();

			_updatingUI = false;
		}

		private void UpdateRenderControls()
		{
			if (_fractalRenderer == null || _renderButton == null || _renderHintLabel == null)
				return;

			if (_fractalRenderer.UseGPURendering)
			{
				_renderButton.Visible = false;
				_renderHintLabel.Text = "GPU mode: live updates";
			}
			else
			{
				_renderButton.Visible = true;
				_renderButton.Disabled = !_pendingCpuRender;
				_renderButton.Text = _pendingCpuRender ? "Render (CPU)" : "Render (CPU)";
				_renderHintLabel.Text = _pendingCpuRender ? "CPU mode: click Render or press R" : "CPU mode: up to date";
			}
		}
	}
}
