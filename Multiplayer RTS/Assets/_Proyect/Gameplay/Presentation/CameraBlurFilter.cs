using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraBlurFilter : MonoBehaviour
{
    public Shader blurShader;
	private Material material;
	//private Camera m_Camera;

    [Range(0, 10)]
    public int iterations = 1;
	//public float desviation;



	void OnEnable() 
    {
		//Object.DestroyImmediate(material);
    }
    private void OnDisable()
    {
		//Object.DestroyImmediate(material);
	}
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (material == null)
		{
			material = new Material(blurShader);
		}

		var temporaryTexture = RenderTexture.GetTemporary(source.width, source.height);
		var temporaryTextureDest = source;
		for (int i = 0; i < iterations; i++)
		{
			
			Graphics.Blit(temporaryTextureDest, temporaryTexture, material, 0);
			Graphics.Blit(temporaryTexture, temporaryTextureDest, material, 1);

		}
		Graphics.Blit(temporaryTextureDest, destination);
		RenderTexture.ReleaseTemporary(temporaryTexture);
		//RenderTexture.ReleaseTemporary(temporaryTextureDest);

	}


	//// Remove command buffers from all cameras we added into
	//private void Cleanup()
	//{
	//	cam.Key.RemoveCommandBuffer(CameraEvent.AfterSkybox, cam.Value);
	//	Object.DestroyImmediate(m_Material);
	//}

	//public void OnEnable()
	//{
	//	Cleanup();
	//}

	//public void OnDisable()
	//{
	//	Cleanup();

	//}

	//// Whenever any camera will render us, add a command buffer to do the work on it
	//public void OnWillRenderObject()
	//{
	//	var act = gameObject.activeInHierarchy && enabled;
	//	if (!act)
	//	{
	//		Cleanup();
	//		return;
	//	}

	//	var cam = Camera.current;
	//	if (!cam)
	//		return;

	//	CommandBuffer buf = null;
	//	// Did we already add the command buffer on this camera? Nothing to do then.
	//	if (m_Cameras.ContainsKey(cam))
	//		return;

	//	if (!m_Material)
	//	{
	//		m_Material = new Material(m_BlurShader);
	//		m_Material.hideFlags = HideFlags.HideAndDontSave;
	//	}

	//	buf = new CommandBuffer();
	//	buf.name = "Grab screen and blur";
	//	m_Cameras[cam] = buf;

	//	// copy screen into temporary RT
	//	int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
	//	buf.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
	//	buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

	//	// get two smaller RTs
	//	int blurredID = Shader.PropertyToID("_Temp1");
	//	int blurredID2 = Shader.PropertyToID("_Temp2");
	//	buf.GetTemporaryRT(blurredID, -2, -2, 0, FilterMode.Bilinear);
	//	buf.GetTemporaryRT(blurredID2, -2, -2, 0, FilterMode.Bilinear);

	//	// downsample screen copy into smaller RT, release screen RT
	//	buf.Blit(screenCopyID, blurredID);
	//	buf.ReleaseTemporaryRT(screenCopyID);

	//	// horizontal blur
	//	buf.SetGlobalVector("offsets", new Vector4(2.0f / Screen.width, 0, 0, 0));
	//	buf.Blit(blurredID, blurredID2, m_Material);
	//	// vertical blur
	//	buf.SetGlobalVector("offsets", new Vector4(0, 2.0f / Screen.height, 0, 0));
	//	buf.Blit(blurredID2, blurredID, m_Material);
	//	// horizontal blur
	//	buf.SetGlobalVector("offsets", new Vector4(4.0f / Screen.width, 0, 0, 0));
	//	buf.Blit(blurredID, blurredID2, m_Material);
	//	// vertical blur
	//	buf.SetGlobalVector("offsets", new Vector4(0, 4.0f / Screen.height, 0, 0));
	//	buf.Blit(blurredID2, blurredID, m_Material);

	//	buf.SetGlobalTexture("_GrabBlurTexture", blurredID);

	//	cam.AddCommandBuffer(CameraEvent.AfterSkybox, buf);
	//}

}
