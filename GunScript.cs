using Godot;
using System;

public partial class GunScript : Node3D
{
	[Export] PackedScene projectile;
	[Export] float speed = 15f, interval = 0.5f;
	float _t;

	public override void _PhysicsProcess(double delta)
	{
		if ((_t += (float)delta) < interval) return;
		_t = 0f;

		var p = projectile.Instantiate<RigidBody3D>();
		GetTree().CurrentScene.AddChild(p);                       
		p.GlobalPosition = GlobalPosition - GlobalBasis.Z * 1f;   
		p.LinearVelocity = -GlobalBasis.Z * speed;                
		p.AddToGroup("projectiles");
		GetTree().CreateTimer(5.0).Timeout += p.QueueFree;       
	}
}
