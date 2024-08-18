render_mode blend_mix;

const float PI = 3.14159265358979323846;

#ifdef TILESET
uniform uvec2 num_tiles = uvec2(1);
#endif
#ifdef TEXTURE_ARRAY
#define SAMPLER sampler2DArray
#define UV_TYPE vec3
uniform uint frame = 0;
#else
#define SAMPLER sampler2D
#define UV_TYPE vec2
#endif
const vec4 ambient = vec4(vec3(0.18), 1.0);
uniform vec4 base_albedo : hint_color;
uniform SAMPLER texture_albedo : hint_albedo;
#ifdef ALBEDO_DECAL
uniform SAMPLER texture_albedo_decal : hint_albedo;
#endif
uniform SAMPLER texture_normal : hint_normal;
uniform float base_metallic : hint_range(0,1) = 0.0;
uniform float base_roughness : hint_range(0,1) = 0.5;
uniform SAMPLER texture_roughness_metallic : hint_white;

varying mat2 basis;
varying vec4 base_color;

const vec2 UV_OFFSETS[4] = {
    vec2(-0.5, -0.5), vec2(-0.5, 0.5), vec2(0.5, 0.5), vec2(0.5, -0.5)
};
const vec3 VIEW_VEC = vec3(0.0, 0.0, 1.0);

const int NUM_SAMPLES = 4;
const float DIV_NUM_SAMPLES = 1.0 / float(NUM_SAMPLES);

void vertex() {
    basis = mat2(normalize(WORLD_MATRIX[0].xy), normalize(WORLD_MATRIX[1].xy));
    base_color = COLOR;
}

UV_TYPE offset_uv(vec2 uv, int i
#ifdef TEXTURE_ARRAY
    , uint frame
#endif
) {
#ifdef TILESET
    uv = uv * vec2(num_tiles);
#endif
    vec2 result = uv + UV_OFFSETS[i] / vec2(128.0, 128.0);
#ifdef TILESET
    result = floor(uv) + (result - floor(result));
    result = result / vec2(num_tiles);
#endif
#ifdef TEXTURE_ARRAY
    return vec3(result, float(frame));
#else
    return result;
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

void fragment() {
    //if (AT_LIGHT_PASS)
    //    COLOR = vec4(1.0);
    //else {
        vec4 color = vec4(0.0);
        for (int i = 0; i < NUM_SAMPLES; ++i) {
            UV_TYPE uv = offset_uv(UV, i
#ifdef TEXTURE_ARRAY
                , frame
#endif
            );
            color += DIV_NUM_SAMPLES * ambient * calc_albedo(uv);
        }
        COLOR = AT_LIGHT_PASS ? vec4(vec3(1.0), color.a) : color;
    //}
}

float distribution_ggx(vec3 normal, vec3 halfway, float roughness) {
    float a2 = roughness * roughness;
    float nh = max(dot(normal, halfway), 0.0);
    float nh2 = nh * nh;
    float denom = nh2 * (a2 - 1.0) + 1.0;
    denom *= PI * denom;
    return a2 / denom;
}

float geometry_schlick_ggx(float nv, float roughness) {
    return nv / (nv * (1.0 - roughness) + roughness);
}

float geometry_smith(vec3 normal, vec3 view, vec3 light, float roughness) {
    float ggx_v = geometry_schlick_ggx(max(dot(normal, view), 0.0), roughness);
    float ggx_l = geometry_schlick_ggx(max(dot(normal, light), 0.0), roughness);
    return ggx_v * ggx_l;
}

vec3 fresnel_schlick(float nv, vec3 f0) {
    return f0 + (1.0 - f0) * pow(1.0 - nv, 5.0);
}

void light() {
    vec4 light_color = vec4(0.0);
    for (int i = 0; i < NUM_SAMPLES; ++i) {
        UV_TYPE uv = offset_uv(UV, i
#ifdef TEXTURE_ARRAY
            , frame
#endif
        );
        vec3 normal = texture(texture_normal, uv).xyz * 2.0 + vec3(-1.0);
        normal.y = -normal.y;
        normal.xy = basis * normal.xy;
        vec3 light = normalize(vec3(-1.0, -3.0, 6.2)); // LIGHT_DIRECTION;
        vec3 halfway = normalize(VIEW_VEC + light);
        vec3 radiance = LIGHT_COLOR.rgb /** LIGHT_ENERGY*/ * PI;
        vec4 albedo = calc_albedo(uv);
        vec4 roughness_metallic = texture(texture_roughness_metallic, uv);
        float roughness = base_roughness * roughness_metallic.g;
        float metallic = base_metallic * roughness_metallic.b;
        
        vec3 f0 = mix(vec3(0.04), albedo.xyz, metallic);
        vec3 fresnel = fresnel_schlick(max(dot(halfway, VIEW_VEC), 0.0), f0);
        float distribution = distribution_ggx(normal, halfway, roughness);
        float geometry = geometry_smith(normal, VIEW_VEC, light, roughness);
        vec3 specular = distribution * geometry * fresnel /
            (4.0 * max(dot(normal, VIEW_VEC), 0.0) * max(dot(normal, light), 0.0) + 0.0001);
            
        vec3 diffuse = (vec3(1.0) - fresnel) * (1.0 - metallic);
        vec4 color = vec4((diffuse * albedo.rgb / PI + specular)
            * radiance * max(dot(normal, light), 0.0), 1.0);
        
        light_color += DIV_NUM_SAMPLES * color;
    }
	LIGHT = light_color;
}
