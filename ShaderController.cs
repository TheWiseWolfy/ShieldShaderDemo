using Godot;
using System;
using System.Collections.Generic;

public partial class ShaderController : MeshInstance3D{
	
	[Export] public bool debug = false;
	[Export] public MeshInstance3D sphereMesh;
	[Export] private ShaderMaterial shader;
	[Export] float decayRate = 1.5f;  
	
	float goldenRatio = (float)(1 + Math.Sqrt(5f)) / 2;

	public int pointCount = 64;
	public float radius = 3f;
	public float rayLength = 5f;
	
	float[] depths = new float[64];
	private Vector3[] points;

	readonly Godot.Collections.Array<Rid> _ignored = new();
	private MultiMesh _mm;
	
	public override void _Ready(){

		points = new Vector3[pointCount];
		foreach (var n in GetTree().GetNodesInGroup("ignored")){
			if (n is CollisionObject3D c) _ignored.Add(c.GetRid());
		}

		//Loading the point positions onto the GPU memery
		for (int i = 0; i < pointCount; i++){
			points[i] = GetPointAtIndex(i) * radius;
			depths[i] = 1f;
		}
		shader.SetShaderParameter("points", points);
		
		{ //debug
			
			var mat = new StandardMaterial3D
			{
				VertexColorUseAsAlbedo = true,
				AlbedoColor = Colors.White // White lets instance colors render without tinting
			};
			
			var sphere = new SphereMesh
			{
				Radius = 0.05f,
				Height = 0.1f,
				Material = mat
			};
			
			_mm = new MultiMesh
			{
				UseColors = true,
				TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
				Mesh = sphere,
				InstanceCount = pointCount,
			};
			
			for (int i = 0; i < pointCount; i++){
				_mm.SetInstanceTransform(i, new Transform3D(Basis.Identity, points[i]));
			}

			AddChild(new MultiMeshInstance3D { Multimesh = _mm });
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_mm.VisibleInstanceCount = debug ? -1 : 0;

		for (int i = 0; i < pointCount; i++)
		{
			_mm.SetInstanceColor(i, new Color(1 - depths[i], 1 - depths[i], 1 - depths[i]));
		}
		
		for (int i = 0; i < depths.Length; i++){
			depths[i] = Mathf.Min(1f, depths[i] + decayRate * (float)delta);
		}
	}

	public override void _PhysicsProcess(double delta){

		var space = GetWorld3D().DirectSpaceState;
		
		for (int i = 0; i < pointCount; i++){
			
			var from = GlobalPosition;
			var point = points[i] ;
			var to    = ToGlobal(point + point.Normalized() * rayLength);   
			
			var q = PhysicsRayQueryParameters3D.Create(from, to);
			q.Exclude = _ignored;
			var hit = space.IntersectRay(q);
			
			if (hit.Count > 0){
				float distance = Mathf.Max(from.DistanceTo((Vector3)hit["position"]) - radius, 0f);
				depths[i] = distance / rayLength;
			}
		}
		
		shader.SetShaderParameter("depths", depths);
	}

	Vector3 GetPointAtIndex(int index){
		float j = index + 0.5f;
		float phi = Mathf.Acos(1 - 2 * j / pointCount);
		float theta = 2 * Mathf.Pi * j / goldenRatio;

		float x = Mathf.Cos(theta) * Mathf.Sin(phi);
		float y = Mathf.Sin(theta) * Mathf.Sin(phi);
		float z = Mathf.Cos(phi);

		Vector3 newPoint = new Vector3(x, y, z);
		return newPoint;
	}
}
