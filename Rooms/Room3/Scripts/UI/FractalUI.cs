using Godot;
using System;
using System.Collections.Generic;
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

	// UI Elements - Shift Parameters (Julia)
	private SpinBox _shiftRealSpinBox;
	private SpinBox _shiftImagSpinBox;
	private Label _winStatusLabel;
	private bool _updatingUI = false; // Prevent feedback loops

		public override void _Ready()
		{
		try
		{
			GD.Print("FractalUI: Starting _Ready...");
			
			GD.Print("FractalUI: Looking for FractalRenderer...");
			_fractalRenderer = GetParent<Node>().FindChild("FractalRenderer", true, false) as FractalRenderer;

			if (_fractalRenderer == null)
			{
				GD.PrintErr("FractalUI: Could not find FractalRenderer node");
				return;
			}
			
			GD.Print("FractalUI: Found FractalRenderer, creating UI panel...");

			// Create UI Panel
			PanelContainer mainPanel = new PanelContainer();
			mainPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat() { BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f) });
			mainPanel.CustomMinimumSize = new Vector2(400, 0);
			AddChild(mainPanel);

			// Create main container
			VBoxContainer mainContainer = new VBoxContainer();
			mainPanel.AddChild(mainContainer);
			
			GD.Print("FractalUI: Creating title and labels...");

			// Create title
			Label titleLabel = new Label { Text = "Division by Zero Fractal" };
			titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			titleLabel.LabelSettings = new LabelSettings();
			titleLabel.LabelSettings.FontSize = 24;
			mainContainer.AddChild(titleLabel);

			// Win status indicator (hidden until victory)
			_winStatusLabel = new Label { Text = "" };
			_winStatusLabel.LabelSettings = new LabelSettings();
			_winStatusLabel.LabelSettings.FontSize = 18;
			_winStatusLabel.Visible = false; // Hidden until victory
			mainContainer.AddChild(_winStatusLabel);

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
			
			GD.Print("FractalUI: Creating shift parameter controls...");

			// Shift Parameters (Division by Zero Parameters)
			mainContainer.AddChild(new Label { Text = "Shift Parameter (Where Division by 0 Occurs):" });

			mainContainer.AddChild(new Label { Text = "Real:" });
			_shiftRealSpinBox = new SpinBox();
			_shiftRealSpinBox.MinValue = -1.5f;
			_shiftRealSpinBox.MaxValue = 1.5f;
			_shiftRealSpinBox.Step = 0.01f;
			_shiftRealSpinBox.ValueChanged += OnShiftChanged;
			mainContainer.AddChild(_shiftRealSpinBox);

			mainContainer.AddChild(new Label { Text = "Imaginary:" });
			_shiftImagSpinBox = new SpinBox();
			_shiftImagSpinBox.MinValue = -1.5f;
			_shiftImagSpinBox.MaxValue = 1.5f;
			_shiftImagSpinBox.Step = 0.01f;
			_shiftImagSpinBox.ValueChanged += OnShiftChanged;
			mainContainer.AddChild(_shiftImagSpinBox);

			// Reset button
			mainContainer.AddChild(new HSeparator());
			Button resetButton = new Button { Text = "Reset to Default" };
			resetButton.Pressed += OnResetPressed;
			mainContainer.AddChild(resetButton);

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

	private void OnFractalTypeChanged(long index)
	{
			// No longer needed - only Julia fractal exists
			_fractalRenderer.SetFractal(new JuliaFractal());
			UpdateUIFromFractal();
		}

		private void OnParameterChanged(double value = 0)
		{
			if (_updatingUI) return;

			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			fractal.MaxIterations = (int)_maxIterationsSpinBox.Value;
			fractal.ColorRange = (float)_colorRangeSlider.Value;
			fractal.ColorShift = (float)_colorShiftSlider.Value;
			fractal.SmoothColoring = _smoothColoringCheckBox.ButtonPressed;

			_fractalRenderer.MarkForUpdate();
		}

		private void OnViewChanged(double value = 0)
		{
			if (_updatingUI) return;

			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal == null) return;

			fractal.CenterPosition = new Math.Complex((float)_panXSpinBox.Value, (float)_panYSpinBox.Value);
			fractal.ZoomLevel = (float)_zoomSpinBox.Value;

			_fractalRenderer.MarkForUpdate();
		}

		private void OnJuliaChanged(double value = 0)
		{
			if (_updatingUI) return;

			if (_fractalRenderer.GetCurrentFractal() is JuliaFractal julia)
			{
				julia.SetShiftParameter((float)_shiftRealSpinBox.Value, (float)_shiftImagSpinBox.Value);
				_fractalRenderer.MarkForUpdate();
			}
		}

		private void OnShiftChanged(double value = 0)
		{
			if (_updatingUI) return;

			if (_fractalRenderer.GetCurrentFractal() is JuliaFractal julia)
			{
				julia.SetShiftParameter((float)_shiftRealSpinBox.Value, (float)_shiftImagSpinBox.Value);
				_fractalRenderer.MarkForUpdate();
			}
		}

		private void OnResetPressed()
		{
			FractalBase fractal = _fractalRenderer.GetCurrentFractal();
			if (fractal != null)
			{
				fractal.Reset();
				_fractalRenderer.MarkForUpdate();
				UpdateUIFromFractal();
			}
		}

		/// <summary>
		/// Updates UI elements to reflect current fractal state.
		/// </summary>
		private void UpdateUIFromFractal()
		{
			_updatingUI = true;

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

			// Update shift parameters (Julia only - always visible now)
			if (fractal is JuliaFractal julia)
			{
				_shiftRealSpinBox.Value = julia.ShiftParameter.Real;
				_shiftImagSpinBox.Value = julia.ShiftParameter.Imaginary;

				// Update win status - only show when victory occurs
				if (julia.DivisionByZeroOccurred)
				{
					_winStatusLabel.Text = "🎉 YOU WIN! Division by Zero Found!";
					_winStatusLabel.LabelSettings.FontColor = new Color(0, 1, 0); // Green
					_winStatusLabel.Visible = true; // Show on victory
				}
				else
				{
					_winStatusLabel.Visible = false; // Hide when not won
				}
			}

			_updatingUI = false;
		}
	}
}
