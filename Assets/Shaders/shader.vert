#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUV;
out vec2 vUV;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;
uniform mat3 uNormalMatrix; // inverse-transpose of the model matrix (3×3)

out vec3 FragPos;  // world-space position
out vec3 Normal;   // world-space normal

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;

    Normal = normalize(uNormalMatrix * aNormal);

    gl_Position = uProj * uView * worldPos;
}
