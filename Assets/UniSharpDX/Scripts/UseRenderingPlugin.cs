using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;


public class UseRenderingPlugin : MonoBehaviour
{
    RenderAPI_D3D11 m_plugin;
    private void OnApplicationQuit()
    {
        if (m_plugin != null)
        {
            m_plugin.Dispose();
            m_plugin = null;
        }
    }

    void Start()
    {
        var tex=CreateTextureAndPassToPlugin();
        var mesh=SendMeshBuffersToPlugin();

        m_plugin = RenderAPI_D3D11.Create(tex, mesh);

        StartCoroutine(CallPluginAtEndOfFrames());
    }

    private Texture2D CreateTextureAndPassToPlugin()
    {
        // Create a texture
        Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        tex.Apply();

        // Set texture onto our material
        GetComponent<Renderer>().material.mainTexture = tex;

        return tex;
    }

    private Mesh SendMeshBuffersToPlugin()
    {
        var filter = GetComponent<MeshFilter>();
        var mesh = filter.mesh;
        // The plugin will want to modify the vertex buffer -- on many platforms
        // for that to work we have to mark mesh as "dynamic" (which makes the buffers CPU writable --
        // by default they are immutable and only GPU-readable).
        mesh.MarkDynamic();

        return mesh;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void OnRenderEventFunc(int eventID);
    OnRenderEventFunc m_onRenderEvent;
    System.IntPtr m_p;
    GCHandle m_onRenderEventHandle;
    public System.IntPtr GetRenderEventFunc()
    {
        if (m_onRenderEvent == null)
        {
            m_onRenderEvent = new OnRenderEventFunc(eventID=>
            {
                if (m_plugin != null)
                {
                    m_plugin.OnRenderEvent(eventID);
                }
            });
            m_onRenderEventHandle=GCHandle.Alloc(m_onRenderEvent);
            m_p = Marshal.GetFunctionPointerForDelegate(m_onRenderEvent);
        }
        return m_p;
    }

    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();

            if(m_plugin == null)
            {
                break;
            }

            // Set time for the plugin
            m_plugin.SetTimeFromUnity(Time.timeSinceLevelLoad);

            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            GL.IssuePluginEvent(GetRenderEventFunc(), 1);
        }
    }
}
