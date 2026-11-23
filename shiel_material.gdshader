shader_type spatial;

render_mode cull_disabled;

//From code
const int POINT_COUNT = 64;
uniform float depths[POINT_COUNT];
uniform vec3 points[POINT_COUNT];

//General
uniform float power = 3.0;
uniform vec4 shield_color : source_color = vec4(0.0, 0.6, 1.0, 1.0);
uniform float influence_range = 3.5;

uniform float cutoff_height = 1.0;
uniform float fade_width = 0.2;

//Textures
uniform vec2 hex_tiling = vec2(4.0, 4.0);
uniform vec2 uv_speed = vec2(0, 0);

uniform sampler2D hex_tex : filter_linear_mipmap, repeat_enable;
uniform sampler2D hex_normal : hint_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D cell_tex : filter_linear_mipmap, repeat_enable;
uniform sampler2D mask : filter_linear_mipmap, repeat_enable;

//Fressnel
uniform float fresnel_shape= 3.0;

//Rim effect
uniform sampler2D depth_tex : hint_depth_texture, repeat_disable, filter_nearest;
uniform float rim_width = 0.1;
uniform float rim_power = 1.0;

uniform float normal_strength = 0.5;


//Wobble effect
uniform sampler2D noise_tex : filter_linear, repeat_enable;
uniform float noise_tiling = 2.0;
uniform float wobble_speed = 2.0;
uniform float wobble_amount = 0.03;

//varying
varying vec3 local_pos;
varying float intensity;

void vertex() {
	local_pos = VERTEX;

	//Intensity as a weighted average
	
	intensity = 0.0;
	for (int i = 0; i < POINT_COUNT; i++) {
		float distanceScalar = distance(VERTEX, points[i]) / influence_range;
		
		if (distanceScalar < 1.0) {
			float potentialIntensity = (1.0 - depths[i]) * (1.0 - distanceScalar);
			intensity = max(intensity, potentialIntensity);
		}
	}

	//Wobble effect
	vec3 dir = normalize(VERTEX);
	float noise_sample =  texture(cell_tex, UV * noise_tiling).r;
	VERTEX += dir * sin(TIME * wobble_speed + noise_sample * TAU) * wobble_amount;
	
	//UV animation
	UV += vec2(TIME * uv_speed.x, TIME * uv_speed.y);
	
}


float getRimValue(vec2 screenUV, mat4 inv_projection_matrix, vec3 vertex){
	float scene_depth = texture(depth_tex, screenUV).r;
	
	vec4 scene_view = inv_projection_matrix * vec4(screenUV * 2.0 - 1.0, scene_depth, 1.0);
	scene_view.xyz /= scene_view.w;

	float gap = (-scene_view.z) - (-vertex.z);      
	float rim = 1.0 - smoothstep(0.0, rim_width, gap);
	return rim;
}

void fragment() {
	//Rim
	float rim =  getRimValue(SCREEN_UV, INV_PROJECTION_MATRIX, VERTEX) * rim_power;

	//Emmision
	float fresnel = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), fresnel_shape) ;
	float hex = texture(hex_tex, UV * hex_tiling).r;
	EMISSION = shield_color.rgb * (power * hex  * intensity * fresnel + rim);
	
	//Albedo
	
	float cell = texture(cell_tex, UV * hex_tiling).r;
	ALBEDO = shield_color.rgb * cell * power;
	
	//Rim effect
	//https://www.youtube.com/watch?v=SiCNFMhDZ1o

	//Normal map
	NORMAL_MAP = texture(hex_normal, UV * hex_tiling).rgb;
	NORMAL_MAP_DEPTH = normal_strength;

	//Alpha
	float maskValue = texture(mask, UV * hex_tiling).r;
	ALPHA = max(intensity, rim) * maskValue * fresnel ;

	//Hiding the top
	float height_mask = 1.0 - smoothstep(cutoff_height - fade_width, cutoff_height, local_pos.y);
	ALPHA = smoothstep(0.0, 1.0, ALPHA * height_mask);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
