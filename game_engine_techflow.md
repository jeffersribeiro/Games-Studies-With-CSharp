# Games Studies With CSharp

## Game Rendering Pipeline: `.gltf` → GPU

### 1. glTF File

* **What it is**

  * A JSON-based format (with optional binary `.glb`) that describes:

    * Mesh geometry (accessors, bufferViews)
    * Materials (PBR metallic-roughness, textures, samplers)
    * Scene graph (nodes, transforms, cameras, lights)
    * Skinning & animations (joints, weights, keyframes)
  * May embed binary blobs directly (via data URIs) or reference external `.bin` and image files.
* **Why it matters**

  * Standardized, efficient “JPEG of 3D”
  * Retains PBR and animation data without custom exporters

---

### 2. MeshLoader (glTF SDK)

* **Responsibilities**

  1. **Parse** the JSON structure into C# objects.
  2. **Load** binary buffer data (decode base64 or read `.bin`).
  3. **Resolve** bufferViews → accessors → typed arrays (floats, ints).
  4. **Build** in-memory `Mesh`/`MeshPrimitive` instances:

     * Positions, normals, tangents, UVs, colors
     * Index arrays (triangles, possibly strips)
  5. **(Optional)** Decode KHR extensions: Draco compression, texture transforms, morph targets, lights.
* **Common libraries**

  * [SharpGLTF](https://github.com/vpenades/SharpGLTF)
  * [glTF2Loader](https://github.com/KhronosGroup/glTF-CSharp-Loader)
  * [AssimpNet](https://github.com/assimp/assimp-net)

---

### 3. Upload to GPU Buffers

> **Bridges CPU data → GPU memory**

* **Generate GL objects**

  ```csharp
  int vao = GL.GenVertexArray();
  int vbo = GL.GenBuffer();
  int ebo = GL.GenBuffer();
  ```
* **Bind & upload**

  ```csharp
  GL.BindVertexArray(vao);

  // Vertex data
  GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
  GL.BufferData(BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw);

  // Index data
  GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
  GL.BufferData(BufferTarget.ElementArrayBuffer,
                indices.Length * sizeof(uint),
                indices,
                BufferUsageHint.StaticDraw);
  ```
* **Set attribute pointers**

  ```csharp
  GL.EnableVertexAttribArray(0); // position
  GL.VertexAttribPointer(0, 3,
                         VertexAttribPointerType.Float,
                         false,
                         3 * sizeof(float), 0);
  // repeat for normals, UVs, etc.
  ```
* **Unbind**

  ```csharp
  GL.BindVertexArray(0);
  ```

---

### 4. Shader Compilation & Setup

* **Compile** your GLSL/HLSL sources on the GPU:

  1. `GL.CreateShader(VertexShader)` → compile → check log
  2. `GL.CreateShader(FragmentShader)` → compile → check log
* **Link** into a program:

  ```csharp
  int program = GL.CreateProgram();
  GL.AttachShader(program, vertex);
  GL.AttachShader(program, fragment);
  GL.LinkProgram(program);
  ```
* **Uniforms & Textures**

  * **Matrices** (model, view, projection)
  * **Material parameters** (albedo color, metallic, roughness)
  * **Texture samplers** (bind at `GL.ActiveTexture`, set uniform to texture unit)
* **State setup** (once or per frame):

  ```csharp
  GL.Enable(EnableCap.DepthTest);
  GL.Enable(EnableCap.CullFace);
  GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
  ```

---

### 5. Draw Call & GPU Pipeline

#### 5.1. Bind & Draw

```csharp
GL.UseProgram(program);
GL.BindVertexArray(vao);
// set uniforms here (e.g. camera matrices)
GL.DrawElements(PrimitiveType.Triangles,
                indexCount,
                DrawElementsType.UnsignedInt,
                0);
```

#### 5.2. Vertex Shader Stage

* **Input**: vertex attributes (position, normal, UV)
* **Work**: transform positions → clip space (`gl_Position`), compute per-vertex outputs (normals, UV)

#### 5.3. Primitive Assembly & Rasterization

* **Assemble** triangles from indexed vertices
* **Rasterize** into fragments (pixels) on screen
* **Interpolation** of per-vertex outputs

#### 5.4. Fragment Shader Stage

* **Input**: interpolated data (normals, UV, colors)
* **Work**: compute final color (lighting, texturing, shadows, post-effects)
* **Output**: `out vec4 FragColor`

#### 5.5. Output Merger (Framebuffer)

* **Blend** with existing pixel (if transparency)
* **Depth test/write** controls occlusion
* **S‐RGB conversion**, **multisampling resolve**

---

## 6. Presentation

* **Swap buffers** (`SwapBuffers()`) to display the rendered frame
* **Loop**: clear → update logic → render → swap

---

### Additional Considerations

* **Animation**

  * Update node transforms per keyframe → upload new `uModel` matrix
* **Skinning**

  * Pass joint matrices array to GPU → vertex shader blends weights
* **Resource Management**

  * Dispose VAOs/VBOs/Shaders when no longer needed
* **Performance**

  * **Batching**: group meshes with same material to reduce `DrawElements` calls
  * **State sorting**: minimize shader and texture switches
* **Debugging**

  * Validate with tools like **RenderDoc**, **glTF-Validator**
  * Check for GL errors: `GL.GetError()`

---

With these steps in place—and a clear understanding of each transition—you’ll have a robust pipeline that takes you from a high-level `.gltf` asset all the way to pixels on screen.
