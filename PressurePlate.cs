using Godot;
using System;

public partial class PressurePlate : Area3D
{
    // Drag your CanvasLayer here in Inspector
    [Export] public CanvasLayer UiLayer;
    
    // Drag your LineEdit here in Inspector
    [Export] public LineEdit InputField;

    [Export] public CollisionShape3D Floor;

    public override void _Ready()
    {
        // Hide UI initially just in case
        if (UiLayer != null) UiLayer.Visible = false;

        // Connect events
        BodyEntered += OnBodyEntered;
        
        // This event triggers when the user presses ENTER in the box
        if (InputField != null)
        {
            InputField.TextSubmitted += OnTextSubmitted;
        }
    }

    private void OnBodyEntered(Node body)
    {
        // Check for Player class (adjust name if yours is 'Joueur')
        if (body is Player) 
        {
            OpenUI();
        }
    }

    private void OpenUI()
    {
        if (UiLayer == null) return;

        UiLayer.Visible = true;
        
        Input.MouseMode = Input.MouseModeEnum.Visible;
        
        InputField.Clear();
        InputField.GrabFocus();
    }

    private void OnTextSubmitted(string text)
    {
        if (int.TryParse(text, out int number))
        {
            DoSomethingWithNumber(number);
            CloseUI();
        }
        else
        {
            GD.Print("Invalid Number! Please type digits only.");
            InputField.Clear();
        }
    }

    private void CloseUI()
    {
        UiLayer.Visible = false;
        
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void DoSomethingWithNumber(int number)
    {
        GD.Print($"User entered: {number}");

        if (number == 5)
        {
            GD.Print("CORRECT!");
            Floor.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        }
        else
        {
            GD.Print("Wrong.");
        }
    }
}