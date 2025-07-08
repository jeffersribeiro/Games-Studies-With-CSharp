#version 330 core

in vec3 FragPos;
in vec3 Normal;

out vec4 FragColor;

uniform vec3 uLightPos;
uniform vec3 uViewPos;

uniform vec3 uLightAmbient;   // e.g. vec3(0.1)
uniform vec3 uLightDiffuse;   // e.g. vec3(0.8)
uniform vec3 uLightSpecular;  // e.g. vec3(1.0)

uniform vec3 uMaterialAmbient;   // often same as object color * some factor
uniform vec3 uMaterialDiffuse;   // base object color
uniform vec3 uMaterialSpecular;  // specular color, often vec3(1.0)
uniform float uMaterialShininess; // e.g. 32.0

void main()
{
    // Ambient
    vec3 ambient = uLightAmbient * uMaterialAmbient;

    // Diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(uLightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = uLightDiffuse * (diff * uMaterialDiffuse);

    // Specular (Blinn–Phong)
    vec3 viewDir = normalize(uViewPos - FragPos);
    vec3 halfDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(norm, halfDir), 0.0), uMaterialShininess);
    vec3 specular = uLightSpecular * (spec * uMaterialSpecular);

    vec3 color = ambient + diffuse + specular;
    FragColor = vec4(color, 1.0);
}
