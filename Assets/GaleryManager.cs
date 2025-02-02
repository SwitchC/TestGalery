using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GaleryManager : MonoBehaviour
{
	public List<string> savedImages = new List<string>();
	public TMP_Text textMeshPro;
    void Update()
{
	if( Input.GetMouseButtonDown( 0 ) )
	{
		if( Input.mousePosition.x < Screen.width / 2 )
		{
			StartCoroutine( TakeScreenshotAndSave() );
		}
		else
		{
			if( NativeGallery.IsMediaPickerBusy() )
				return;

			if( Input.mousePosition.x > Screen.width / 2 )
			{
				PickImage( 2024 );
			}
		}
	}
}
private async void RequestPermissionAsynchronously( NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaTypes )
{
	NativeGallery.Permission permission = await NativeGallery.RequestPermissionAsync( permissionType, mediaTypes );
	Debug.Log( "Permission result: " + permission );
}

private IEnumerator TakeScreenshotAndSave()
{
	yield return new WaitForEndOfFrame();

	Texture2D ss = new Texture2D( Screen.width, Screen.height, TextureFormat.RGB24, false );
	ss.ReadPixels( new Rect( 0, 0, Screen.width, Screen.height ), 0, 0 );
	ss.Apply();

	NativeGallery.Permission permission = NativeGallery.SaveImageToGallery( ss, "GalleryTest", "Image.png",
	( success, path ) =>
	{ 
		Debug.Log( "Media save result: " + success + " " + path );
		savedImages.Add(path);
		textMeshPro.text = "Saved with path: " + path;
	});

	Debug.Log( "Permission result: " + permission );

	Destroy( ss );
}

private void PickImage( int maxSize )
{
	NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) =>
	{
		Debug.Log( "Image path: " + path );
		if(savedImages.Find( ( p ) => p == path) != null)
		{
			textMeshPro.text = "Image " +path+ " is from this app";
		}
		else
		{ 
			textMeshPro.text = "Image " +path+ " is not from this app";
		}
		if( path != null )
		{
			Texture2D texture = NativeGallery.LoadImageAtPath( path, maxSize );
			if( texture == null )
			{
				Debug.Log( "Couldn't load texture from " + path );
				return;
			}

			GameObject quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
			quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1f;
			quad.transform.forward = Camera.main.transform.forward;
			quad.transform.localScale = new Vector3( 1f, texture.height / (float) texture.width, 1f );

			Material material = quad.GetComponent<Renderer>().material;
			if( !material.shader.isSupported )
				material.shader = Shader.Find( "Legacy Shaders/Diffuse" );

			material.mainTexture = texture;

			Destroy( quad, 5f );

			Destroy( texture, 5f );
		}
	} );

	Debug.Log( "Permission result: " + permission );
}
}
