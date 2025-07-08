#version 330 core

layout(location = 0) in vec3 aPosition;   // vertex position
layout(location = 1) in vec3 aNormal;     // vertex normal
layout(location = 2) in vec2 aTexCoord;   // vertex UV

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;

void main()
{
    FragPos = vec3(uModel * vec4(aPosition, 1.0));
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    TexCoord = aTexCoord;

    gl_Position = uProj * uView * vec4(FragPos, 1.0);
}
