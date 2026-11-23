using Godot;
using System;

public partial class ShieldScript : Area3D
{
	public override void _Ready()
	{
		BodyEntered += b => {
			GD.Print("Shield hit");
			if (b is not RigidBody3D rb) return;
			var n  = (rb.GlobalPosition - GlobalPosition).Normalized();   
			var vn = rb.LinearVelocity.Dot(n);
			if (vn < 0f) rb.ApplyCentralImpulse(-2f * vn * n * rb.Mass);  
		};
	}

	public override void _Process(double delta)
	{
	}
	

}
