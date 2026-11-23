using Godot;
using System;

public partial class SimplePlayer : RigidBody3D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var i = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		LinearVelocity = new Vector3(i.X * 6f, LinearVelocity.Y, i.Y * 6f);
	}
}
