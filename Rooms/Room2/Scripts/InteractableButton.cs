using Godot;
using System;

public partial class InteractableButton : StaticBody3D, IInteractable
{
	[Export] private CollisionShape3D _chairCollision;
	[Export] private CollisionShape3D _floorCollision;

	public void Interact()
	{
		if (_chairCollision != null && _floorCollision != null)
		{
			// IMPORTANT : On utilise SetDeferred pour éviter les erreurs de physique
			// On passe la propriété "disabled" à true
			_chairCollision.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
			_floorCollision.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

			GD.Print("Collision désactivée ! Le mur est maintenant un bug.");

			// Optionnel : Changer l'apparence pour montrer que c'est "glitché"
			// (ex: baisser l'opacité si l'objet a un matériau transparent)
		}
	}
}
