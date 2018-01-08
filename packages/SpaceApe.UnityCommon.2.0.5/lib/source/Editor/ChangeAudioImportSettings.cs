using UnityEngine;

using UnityEditor;
/*
 * TODO:  disabled because most of them are no longer supported by Unity 5. Please fix them all.


// /////////////////////////////////////////////////////////////////////////////////////////////////////////

//

// Batch audio import settings modifier.

//

// Modifies all selected audio clips in the project window and applies the requested modification on the

// audio clips. Idea was to have the same choices for multiple files as you would have if you open the

// import settings of a single audio clip. Put this into Assets/Editor and once compiled by Unity you find

// the new functionality in Custom -> Sound. Enjoy! :-)

//

// April 2010. Based on Martin Schultz's texture import settings batch modifier.

//

// /////////////////////////////////////////////////////////////////////////////////////////////////////////

public class ChangeAudioImportSettings : ScriptableObject {
	
	
	
	[MenuItem ("SpaceApe/Sound/Toggle audio compression/Disable")]
	
	static void ToggleCompression_Disable() {
		
		SelectedToggleCompressionSettings(AudioImporterFormat.Native);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Toggle audio compression/Enable")]
	
	static void ToggleCompression_Enable() {
		
		SelectedToggleCompressionSettings(AudioImporterFormat.Compressed);
		
	}
	
	
	
	// ----------------------------------------------------------------------------
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/32")]
	
	static void SetCompressionBitrate_32kbps() {
		
		SelectedSetCompressionBitrate(32000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/64")]
	
	static void SetCompressionBitrate_64kbps() {
		
		SelectedSetCompressionBitrate(64000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/96")]
	
	static void SetCompressionBitrate_96kbps() {
		
		SelectedSetCompressionBitrate(96000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/128")]
	
	static void SetCompressionBitrate_128kbps() {
		
		SelectedSetCompressionBitrate(128000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/144")]
	
	static void SetCompressionBitrate_144kbps() {
		
		SelectedSetCompressionBitrate(144000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/156 (default)")]
	
	static void SetCompressionBitrate_156kbps() {
		
		SelectedSetCompressionBitrate(156000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/160")]
	
	static void SetCompressionBitrate_160kbps() {
		
		SelectedSetCompressionBitrate(160000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/192")]
	
	static void SetCompressionBitrate_192kbps() {
		
		SelectedSetCompressionBitrate(192000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/224")]
	
	static void SetCompressionBitrate_224kbps() {
		
		SelectedSetCompressionBitrate(224000);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Set audio compression bitrate (kbps)/240")]
	
	static void SetCompressionBitrate_240kbps() {
		
		SelectedSetCompressionBitrate(240000);
		
	}
	
	
	
	// ----------------------------------------------------------------------------
	
	
	
	[MenuItem ("SpaceApe/Sound/load type/Stream from disc")]
	
	static void ToggleDecompressOnLoad_Disable() {
		
		SelectedToggleDecompressOnLoadSettings(AudioClipLoadType.Streaming);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/load type/Descompress on Load")]
	
	static void ToggleDecompressOnLoad_Enable() {
		
		SelectedToggleDecompressOnLoadSettings(AudioClipLoadType.DecompressOnLoad);
		
	} 
	
	
	
	[MenuItem ("SpaceApe/Sound/load type/CompressedInMemory")]
	
	static void ToggleDecompressOnLoad_Enable2() {
		
		SelectedToggleDecompressOnLoadSettings(AudioClipLoadType.CompressedInMemory);
		
	}
	
	
	
	// ----------------------------------------------------------------------------
	
	
	
	[MenuItem ("SpaceApe/Sound/Toggle 3D sound/Disable")]
	
	static void Toggle3DSound_Disable() {
		
		SelectedToggle3DSoundSettings(false);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Toggle 3D sound/Enable")]
	
	static void Toggle3DSound_Enable() {
		
		SelectedToggle3DSoundSettings(true);
		
	}
	
	
	
	// ----------------------------------------------------------------------------
	
	
	
	[MenuItem ("SpaceApe/Sound/Toggle mono/Auto")]
	
	static void ToggleForceToMono_Auto() {
		
		SelectedToggleForceToMonoSettings(false);
		
	}
	
	
	
	[MenuItem ("SpaceApe/Sound/Toggle mono/Forced")]
	
	static void ToggleForceToMono_Forced() {
		
		SelectedToggleForceToMonoSettings(true);
		
	}
	
	
	
	// ----------------------------------------------------------------------------
	
	[MenuItem ("SpaceApe/Sound/Hardware Decoding/Enabled")]
	
	static void enable_Hardware_yes() {
		
		enableHardwareDecoding(true);
		
	}
	
	[MenuItem ("SpaceApe/Sound/Hardware Decoding/Disabled")]
	
	static void enable_Hardware_no() {
		
		enableHardwareDecoding(false);
		
	}
	
	
	
	
	
	
	
	static void enableHardwareDecoding ( bool enable )
		
	{
		
		Object[] audioclips = GetSelectedAudioclips();
		
		Selection.objects = new Object[0];
		
		foreach (AudioClip audioclip in audioclips) {
			
			string path = AssetDatabase.GetAssetPath(audioclip);
			
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			
			audioImporter.hardware = enable;
			
			AssetDatabase.ImportAsset(path);
			
		}
		
	}
	
	
	
	static void SelectedToggleCompressionSettings(AudioImporterFormat newFormat) {
		
		
		
		Object[] audioclips = GetSelectedAudioclips();
		
		Selection.objects = new Object[0];
		
		foreach (AudioClip audioclip in audioclips) {
			
			string path = AssetDatabase.GetAssetPath(audioclip);
			
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			
			//audioImporter.format = newFormat;
			
			AssetDatabase.ImportAsset(path);
			
		}
		
	}
	
	
	
	static void SelectedSetCompressionBitrate(float newCompressionBitrate) {
		
		
		
		Object[] audioclips = GetSelectedAudioclips();
		
		Selection.objects = new Object[0];
		
		foreach (AudioClip audioclip in audioclips) {
			
			string path = AssetDatabase.GetAssetPath(audioclip);
			
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			
			// audioImporter.compressionBitrate = (int)newCompressionBitrate;
			
			AssetDatabase.ImportAsset(path);
			
		}
		
	}
	
	
	
	static void SelectedToggleDecompressOnLoadSettings(AudioClipLoadType enabled) {
		
		
		
		Object[] audioclips = GetSelectedAudioclips();
		
		Selection.objects = new Object[0];
		
		foreach (AudioClip audioclip in audioclips) {
			
			string path = AssetDatabase.GetAssetPath(audioclip);
			
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			
			// audioImporter.loadType = enabled;
			
			AssetDatabase.ImportAsset(path);
			
		}
		
	}
	
	
	
	static void SelectedToggle3DSoundSettings(bool enabled) {
		
		
		
		Object[] audioclips = GetSelectedAudioclips();
		
		Selection.objects = new Object[0];
		
		foreach (AudioClip audioclip in audioclips) {
			
			string path = AssetDatabase.GetAssetPath(audioclip);
			
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			
			audioImporter.threeD = enabled;
			
			AssetDatabase.ImportAsset(path);
			
		}
		
	}
	
	
	
	static void SelectedToggleForceToMonoSettings(bool enabled) {
		
		
		
		Object[] audioclips = GetSelectedAudioclips();
		
		Selection.objects = new Object[0];
		
		foreach (AudioClip audioclip in audioclips) {
			
			string path = AssetDatabase.GetAssetPath(audioclip);
			
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			
			audioImporter.forceToMono = enabled;
			
			AssetDatabase.ImportAsset(path);
			
		}
		
	}
	
	
	
	static Object[] GetSelectedAudioclips()
		
	{
		
		return Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
		
	}
	
}
*/