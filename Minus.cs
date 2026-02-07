using System;
using Godot;

public partial class Minus : Area3D
{
    [Export] public Label3D Label;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Player) return;
        UpdateText((ushort) (_getValue() - 1));
    }

    private ushort _getValue()
    {
        return (ushort)(Label != null ? Convert.ToUInt32(Label.Text) : 0);
    }
    
    private void UpdateText(ushort value)
    {
        if (Label != null)
        {
            Label.Text = value.ToString();
        }
    }
}