render_mode blend_mix;

const float PI = 3.14159265358979323846;

#define SAMPLER sampler2D
#define UV_TYPE vec2

uniform sampler2D attributes;
uniform vec4 base_albedo : hint_color;
uniform SAMPLER texture_albedo : hint_albedo;
#ifdef ALBEDO_DECAL
uniform SAMPLER texture_albedo_decal : hint_albedo;
#endif

#ifndef SHADELESS
const vec4 ambient = vec4(vec3(0.18), 1.0);
uniform SAMPLER texture_normal : hint_normal;
uniform float base_metallic : hint_range(0,1) = 0.0;
uniform float base_roughness : hint_range(0,1) = 0.5;
uniform SAMPLER texture_roughness_metallic : hint_white;
uniform vec4 base_emission : hint_color = vec4(vec3(0.0), 1.0);
uniform SAMPLER texture_emission : hint_white;
#endif

#ifdef UV_TRANSFORM
uniform mat4 uv_transform = mat4(1.0);
#endif

#ifdef ATTRIBUTE_WEIGHTS
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
#endif

varying mat2 basis;
varying vec4 base_color;
varying vec2 transformed_uv;
#ifndef SHADELESS
varying vec3 normal;
#endif

const vec3 VIEW_VEC = vec3(0.0, 0.0, 1.0);
const vec4 FIXED_LIGHT_COLOR = vec4(1.0, 1.0, 1.0, 1.0);
const vec3 FIXED_LIGHT_DIRECTION = vec3(-1.0, -3.0, 6.2);

void vertex() {
    base_color = vec4(1.0);
    uint index = uint(COLOR.r * 255.0)         + (uint(COLOR.g * 255.0) << 8u) +
                (uint(COLOR.b * 255.0) << 16u) + (uint(COLOR.a * 255.0) << 24u);
#ifndef SHADELESS
	normal = texelFetch(attributes, ivec2(int(index), ATTRIBUTE_NORMAL), 0).xyz;
#endif
#ifdef ATTRIBUTE_WEIGHTS
    vec4 weights_0 = texelFetch(attributes, ivec2(int(index), ATTRIBUTE_WEIGHTS + 0), 0);
    vec4 weights_1 = texelFetch(attributes, ivec2(int(index), ATTRIBUTE_WEIGHTS + 1), 0);
    vec4 weights_2 = texelFetch(attributes, ivec2(int(index), ATTRIBUTE_WEIGHTS + 2), 0);
    vec4 weights_3 = texelFetch(attributes, ivec2(int(index), ATTRIBUTE_WEIGHTS + 3), 0);
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
#ifndef SHADELESS
    basis = mat2(normalize(WORLD_MATRIX[0].xy), normalize(WORLD_MATRIX[1].xy));
    normal = (rig_transform * vec4(normal, 0.0)).xyz;
    normal.xy = basis * normal.xy;
#endif
    VERTEX = (rig_transform * vec4(VERTEX, 0.0, 1.0)).xy;
#else
#ifndef SHADELESS
    basis = mat2(normalize(WORLD_MATRIX[0].xy), normalize(WORLD_MATRIX[1].xy));
    normal.xy = basis * normal.xy;
#endif
#endif
#ifdef UV_TRANSFORM
    transformed_uv = (uv_transform * vec4(UV, 0.0, 1.0)).xy;
#else
    transformed_uv = UV;
#endif
}

vec4 calc_albedo(UV_TYPE uv) {
#ifdef ALBEDO_DECAL
    vec4 decal = texture(texture_albedo_decal, uv);
    return mix(base_color * base_albedo * texture(texture_albedo, uv),
        vec4(decal.rgb, 1.0),
        decal.a);
#endif
    return base_color * base_albedo * texture(texture_albedo, uv);
}

#ifndef SHADELESS
float distribution_ggx(vec3 normal_, vec3 halfway, float roughness) {
    float a2 = roughness * roughness;
    float nh = max(dot(normal_, halfway), 0.0);
    float nh2 = nh * nh;
    float denom = nh2 * (a2 - 1.0) + 1.0;
    denom *= PI * denom;
    return a2 / denom;
}

float geometry_schlick_ggx(float nv, float roughness) {
    return nv / (nv * (1.0 - roughness) + roughness);
}

float geometry_smith(vec3 normal_, vec3 view, vec3 light, float roughness) {
    float ggx_v = geometry_schlick_ggx(max(dot(normal_, view), 0.0), roughness);
    float ggx_l = geometry_schlick_ggx(max(dot(normal_, light), 0.0), roughness);
    return ggx_v * ggx_l;
}

vec3 fresnel_schlick(float nv, vec3 f0) {
    return f0 + (1.0 - f0) * pow(1.0 - nv, 5.0);
}

vec4 calc_light_color(UV_TYPE uv) {
    vec3 tex_normal = texture(texture_normal, uv).xyz * 2.0 + vec3(-1.0);
    tex_normal.y = -tex_normal.y;
    tex_normal.xy = basis * tex_normal.xy;
    vec3 real_normal = mat3(vec3(1.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0), normal) * tex_normal;

    vec3 light = normalize(FIXED_LIGHT_DIRECTION); // LIGHT_DIRECTION;
    vec3 halfway = normalize(VIEW_VEC + light);
    vec3 radiance = FIXED_LIGHT_COLOR.rgb /** LIGHT_ENERGY*/ * PI;
    vec4 albedo = calc_albedo(uv);
    vec4 roughness_metallic = texture(texture_roughness_metallic, uv);
    float roughness = base_roughness * roughness_metallic.g;
    float metallic = base_metallic * roughness_metallic.b;
    
    vec3 f0 = mix(vec3(0.04), albedo.xyz, metallic);
    vec3 fresnel = fresnel_schlick(max(dot(halfway, VIEW_VEC), 0.0), f0);
    float distribution = distribution_ggx(real_normal, halfway, roughness);
    float geometry = geometry_smith(real_normal, VIEW_VEC, light, roughness);
    vec3 specular = distribution * geometry * fresnel /
        (4.0 * max(dot(real_normal, VIEW_VEC), 0.0) * max(dot(real_normal, light), 0.0) + 0.0001);
        
    vec3 diffuse = (vec3(1.0) - fresnel) * (1.0 - metallic);
    vec4 emission4 = base_emission * texture(texture_emission, uv);
    vec3 emission = emission4.rgb * emission4.a;
    vec4 color = vec4((diffuse * albedo.rgb / PI + specular)
        * radiance * max(dot(real_normal, light), 0.0) + emission, 1.0);
    
    return color;
}
#endif

void fragment() {
    #ifdef SHADELESS
    COLOR = calc_albedo(transformed_uv);
    #else
    vec4 color = ambient * calc_albedo(transformed_uv);
    COLOR = color + calc_light_color(transformed_uv);
    COLOR.a = min(COLOR.a, 1.0);
    #endif
}
