shader_type canvas_item;
render_mode blend_mix;
const float PI = 3.14159265358979323846;
uniform sampler2D attributes;
uniform vec4 base_albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform mat4 rig_transform_00 = mat4(1.0);
uniform mat4 rig_transform_01 = mat4(1.0);
uniform mat4 rig_transform_02 = mat4(1.0);
uniform mat4 rig_transform_03 = mat4(1.0);
uniform mat4 rig_transform_04 = mat4(1.0);
uniform mat4 rig_transform_05 = mat4(1.0);
uniform mat4 rig_transform_06 = mat4(1.0);
uniform mat4 rig_transform_07 = mat4(1.0);
uniform mat4 rig_transform_08 = mat4(1.0);
uniform mat4 rig_transform_09 = mat4(1.0);
uniform mat4 rig_transform_10 = mat4(1.0);
uniform mat4 rig_transform_11 = mat4(1.0);
uniform mat4 rig_transform_12 = mat4(1.0);
uniform mat4 rig_transform_13 = mat4(1.0);
uniform mat4 rig_transform_14 = mat4(1.0);
uniform mat4 rig_transform_15 = mat4(1.0);
varying mat2 basis;
varying vec4 base_color;
varying vec2 transformed_uv;
const vec3 VIEW_VEC = vec3(0.0, 0.0, 1.0);
const vec4 FIXED_LIGHT_COLOR = vec4(1.0, 1.0, 1.0, 1.0);
const vec3 FIXED_LIGHT_DIRECTION = vec3(-1.0, -3.0, 6.2);
void vertex() {
    base_color = vec4(1.0);
    uint index = uint(COLOR.r * 255.0) + (uint(COLOR.g * 255.0) << 8u) +
                (uint(COLOR.b * 255.0) << 16u) + (uint(COLOR.a * 255.0) << 24u);
    vec4 weights_0 = texelFetch(attributes, ivec2(int(index), 0 + 0), 0);
    vec4 weights_1 = texelFetch(attributes, ivec2(int(index), 0 + 1), 0);
    vec4 weights_2 = texelFetch(attributes, ivec2(int(index), 0 + 2), 0);
    vec4 weights_3 = texelFetch(attributes, ivec2(int(index), 0 + 3), 0);
    mat4 rig_transform =
        rig_transform_00 * weights_0[0] +
        rig_transform_01 * weights_0[1] +
        rig_transform_02 * weights_0[2] +
        rig_transform_03 * weights_0[3] +
        rig_transform_04 * weights_1[0] +
        rig_transform_05 * weights_1[1] +
        rig_transform_06 * weights_1[2] +
        rig_transform_07 * weights_1[3] +
        rig_transform_08 * weights_2[0] +
        rig_transform_09 * weights_2[1] +
        rig_transform_10 * weights_2[2] +
        rig_transform_11 * weights_2[3] +
        rig_transform_12 * weights_3[0] +
        rig_transform_13 * weights_3[1] +
        rig_transform_14 * weights_3[2] +
        rig_transform_15 * weights_3[3];
    VERTEX = (rig_transform * vec4(VERTEX, 0.0, 1.0)).xy;
    transformed_uv = UV;
}
vec4 calc_albedo(vec2 uv) {
    return base_color * base_albedo * texture(texture_albedo, uv);
}
void fragment() {
    COLOR = calc_albedo(transformed_uv);
}
