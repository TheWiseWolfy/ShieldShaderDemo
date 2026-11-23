using Godot;
using System;

public partial class Camera : Camera3D {
	
	float _yaw, _pitch = -0.4f, _dist = 6f;
	private float _zoom = 6f;
	private Vector3 _pivot;
	
	public override void _Ready()
	{
		TopLevel = true; 
		//Input.MouseMode = Input.MouseModeEnum.Captured; 
	}

	
	public override void _UnhandledInput(InputEvent e)
	{
		if (e is InputEventMouseMotion m && Input.IsMouseButtonPressed(MouseButton.Right)) {
			_yaw -= m.Relative.X * 0.003f;
			_pitch = Mathf.Clamp(_pitch - m.Relative.Y * 0.003f, -1.2f, 0.3f);
		}
		if (e is InputEventMouseButton b && b.Pressed) {
			if (b.ButtonIndex == MouseButton.WheelUp)   _zoom = Mathf.Max(2f, _zoom - 0.5f);
			if (b.ButtonIndex == MouseButton.WheelDown) _zoom = Mathf.Min(15f, _zoom + 0.5f);
		}
	}

	
	public override void _Process(double d)
	{
		var target = GetParent<Node3D>().GlobalPosition + Vector3.Up;
		
		_pivot = _pivot.Lerp(target, 1f - Mathf.Exp(-10f * (float)d));    
		_dist = Mathf.Lerp(_dist, _zoom, 1f - Mathf.Exp(-8f * (float)d));
		GlobalPosition = _pivot + Basis.FromEuler(new Vector3(_pitch, _yaw, 0)) * new Vector3(0, 0, _dist);
		LookAt(_pivot);
	}
}
