shader_type canvas_item;

varying vec2 pos;
uniform vec2 dimensions = vec2(1920.0, 1080.0);
uniform float radius = 0.5;

void vertex() {
	pos = VERTEX.xy;
}

void fragment() {
	float dist = distance(pos, dimensions / 2.0);
	float true_radius = radius * (length(dimensions / 2.0) + 2.0) - 1.0;
	float alpha = clamp(dist - true_radius, 0.0, 1.0);
	COLOR.a = alpha;
}