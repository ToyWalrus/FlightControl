shader_type canvas_item;

uniform vec3 color : source_color = vec3(0);

void fragment() {
	COLOR = texture(TEXTURE, UV);
	COLOR.b = color.b;
	COLOR.r = color.r;
	COLOR.g = color.g;
}
