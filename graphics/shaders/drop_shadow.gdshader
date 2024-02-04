shader_type canvas_item;
render_mode blend_mix;

uniform vec2 offset = vec2(8.0, 8.0);
uniform vec4 shadow_color : source_color;

// https://github.com/godotengine/godot-demo-projects/tree/3.5-9e68af3/2d/sprite_shaders/shaders

void fragment() {
	vec2 ps = TEXTURE_PIXEL_SIZE;

	vec4 shadow = vec4(shadow_color.rgb, texture(TEXTURE, UV - offset * ps).a * shadow_color.a);
	vec4 col = texture(TEXTURE, UV);

	COLOR = mix(shadow, col, col.a);
}