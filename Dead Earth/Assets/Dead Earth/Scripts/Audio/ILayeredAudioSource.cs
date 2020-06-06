public interface ILayeredAudioSource
{
	bool Play(AudioCollection pool, int bank, int layer, bool looping = true);
	void Stop(int layerIndex);
	void Mute(int layerIndex, bool mute);
	void Mute(bool mute);
}