using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] private RayCast3D _rayCast;

	public float Speed = 5.0f;
	public float JumpVelocity = 6.5f;

	// Sensibilité de la souris (ajustable dans l'inspecteur)
	public float MouseSensitivity = 0.003f;

	// Référence à la caméra
	private Camera3D _camera;

	public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		// 1. On récupère la caméra (assure-toi qu'elle s'appelle bien "Camera3D" dans ta scène)
		_camera = GetNode<Camera3D>("Camera3D");

		// 2. On capture la souris (elle disparaît et reste au centre)
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Cette fonction gère les événements (clics, mouvements souris) qui ne sont pas du gameplay direct
	public override void _UnhandledInput(InputEvent @event)
	{
		// Si l'événement est un mouvement de souris
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			// Rotation HEOIZONTALE (Y) du CORPS entier
			RotateY(-eventMouseMotion.Relative.X * MouseSensitivity);

			// Rotation VERTICALE (X) de la CAMÉRA seulement
			_camera.RotateX(-eventMouseMotion.Relative.Y * MouseSensitivity);

			// Bloquer la rotation verticale pour ne pas se tordre le cou (entre -90 et 90 degrés)
			Vector3 cameraRot = _camera.Rotation;
			cameraRot.X = Mathf.Clamp(cameraRot.X, Mathf.DegToRad(-90), Mathf.DegToRad(90));
			_camera.Rotation = cameraRot;
		}

		// Permettre de quitter proprement avec Echap
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= Gravity * (float)delta;

		if (Input.IsActionJustPressed("Sauter") && IsOnFloor())
			velocity.Y = JumpVelocity;

		Vector2 inputDir = Input.GetVector("Gauche", "Droite", "Avancer", "Reculer");

		// Note importante : On utilise maintenant la direction locale du joueur
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		// On vérifie si c'est un clic gauche
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print("click !");
			CheckInteraction();
		}
	}

	private void CheckInteraction()
	{
		if (_rayCast.IsColliding())
		{
			// On récupère l'objet touché
			var collider = _rayCast.GetCollider();

			GD.Print("élément touché récupéré !");
			GD.Print($"Touché : {((Node)collider).Name} | Type : {collider.GetType().Name}");
			// On vérifie si cet objet implémente notre interface
			if (collider is IInteractable interactable)
			{
				GD.Print("appel de l'interaction !");
				interactable.Interact();
			}
		}
	}
}
