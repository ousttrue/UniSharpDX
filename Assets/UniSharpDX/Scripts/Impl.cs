using System;


public static class Impl
{
    // Native plugin rendering events are only called if a plugin is used
    // by some script. This means we have to DllImport at least
    // one function in some active script.
    // For this example, we'll call into plugin's SetTimeFromUnity
    // function and pass the current time so the plugin can animate.
    public static void SetTimeFromUnity(float t)
    {
        throw new NotImplementedException();
    }

    // We'll also pass native pointer to a texture in Unity.
    // The plugin will fill texture data from native code.
    public static void SetTextureFromUnity(System.IntPtr texture, int w, int h)
    {
        throw new NotImplementedException();
    }

    // We'll pass native pointer to the mesh vertex buffer.
    // Also passing source unmodified mesh data.
    // The plugin will fill vertex data from native code.
    public static void SetMeshBuffersFromUnity(IntPtr vertexBuffer, int vertexCount, IntPtr sourceVertices, IntPtr sourceNormals, IntPtr sourceUVs)
    {
        throw new NotImplementedException();
    }

    public static IntPtr GetRenderEventFunc()
    {
        throw new NotImplementedException();
    }
}
