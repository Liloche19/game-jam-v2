using Godot;

public partial class ButtonTrigger : Area3D
{
    [Export] public NodePath FloorToDisable;

    private bool _activated = false;

    private void OnBodyEntered(Node body)
    {
        if (_activated)
            return;

        if (body is CharacterBody3D player)
        {
            // Vérifie que le joueur DESCEND (donc qu’il saute)
            if (player.Velocity.Y < -1.0f)
            {
                _activated = true;

                var floor = GetNode<StaticBody3D>(FloorToDisable);
                floor.QueueFree(); // ou désactiver collision

                GD.Print("Button activated: floor removed (feature)");
            }
        }
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
}
