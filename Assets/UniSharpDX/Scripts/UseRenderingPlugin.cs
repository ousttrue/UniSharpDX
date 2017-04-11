using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;


public class UseRenderingPlugin : MonoBehaviour
{
    IEnumerator Start()
    {
        CreateTextureAndPassToPlugin();
        SendMeshBuffersToPlugin();
        yield return StartCoroutine("CallPluginAtEndOfFrames");
    }

    private void CreateTextureAndPassToPlugin()
    {
        // Create a texture
        Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        tex.Apply();

        // Set texture onto our material
        GetComponent<Renderer>().material.mainTexture = tex;

        // Pass texture pointer to the plugin
        Impl.SetTextureFromUnity(tex.GetNativeTexturePtr(), tex.width, tex.height);
    }

    private void SendMeshBuffersToPlugin()
    {
        var filter = GetComponent<MeshFilter>();
        var mesh = filter.mesh;
        // The plugin will want to modify the vertex buffer -- on many platforms
        // for that to work we have to mark mesh as "dynamic" (which makes the buffers CPU writable --
        // by default they are immutable and only GPU-readable).
        mesh.MarkDynamic();

        // However, mesh being dynamic also means that the CPU on most platforms can not
        // read from the vertex buffer. Our plugin also wants original mesh data,
        // so let's pass it as pointers to regular C# arrays.
        // This bit shows how to pass array pointers to native plugins without doing an expensive
        // copy: you have to get a GCHandle, and get raw address of that.
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;
        GCHandle gcVertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        GCHandle gcNormals = GCHandle.Alloc(normals, GCHandleType.Pinned);
        GCHandle gcUV = GCHandle.Alloc(uvs, GCHandleType.Pinned);

        Impl.SetMeshBuffersFromUnity(mesh.GetNativeVertexBufferPtr(0), mesh.vertexCount, gcVertices.AddrOfPinnedObject(), gcNormals.AddrOfPinnedObject(), gcUV.AddrOfPinnedObject());

        gcVertices.Free();
        gcNormals.Free();
        gcUV.Free();
    }


    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();

            // Set time for the plugin
            Impl.SetTimeFromUnity(Time.timeSinceLevelLoad);

            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            GL.IssuePluginEvent(Impl.GetRenderEventFunc(), 1);
        }
    }
}
