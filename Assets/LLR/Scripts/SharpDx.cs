using UnityEngine;


public class SharpDx : MonoBehaviour
{
    void Start()
    {
        var tex = new Texture2D(640, 480);

        using (var t = new SharpDX.Direct3D11.Texture2D(tex.GetNativeTexturePtr()))
        {
            var desc=t.Description;
            Debug.Log(desc.Width);
            int a = 0;
        }
    }
}
