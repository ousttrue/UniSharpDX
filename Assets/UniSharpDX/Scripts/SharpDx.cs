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

        var backBufferDesc = new SharpDX.DXGI.ModeDescription(1920, 1080
            , new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm);
    }
}
