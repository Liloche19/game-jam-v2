using Godot;
using System;

public partial class EndButton : StaticBody3D, IInteractable
{
    [Export] private CollisionShape3D _floorCollision;

    public void Interact()
    {
        GD.Print("Interaction avec le bouton de fin !");
        if (_floorCollision != null)
        {
            // IMPORTANT : On utilise SetDeferred pour éviter les erreurs de physique
            // On passe la propriété "disabled" à true
            _floorCollision.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

            GD.Print("Collision désactivée ! Le mur est maintenant un bug.");

            // Optionnel : Changer l'apparence pour montrer que c'est "glitché"
            // (ex: baisser l'opacité si l'objet a un matériau transparent)
        }
    }
}
