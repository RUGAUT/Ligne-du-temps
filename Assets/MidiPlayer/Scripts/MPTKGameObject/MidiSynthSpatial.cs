//#define MPTK_PRO

using UnityEngine;

#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
using Oboe.Stream;
#endif

namespace MidiPlayerTK
{

    /// <summary> 
    /// Base class which contains all the stuff to build a Wave Table Synth.
    /// 
    /// Loads SoundFont and samples, process midi event, play voices, controllers, generators ...\n 
    /// This class is inherited by others class to build these prefabs: MidiStreamPlayer, MidiFilePlayer, MidiInReader.\n
    /// <b>It is not recommended to instanciate directly this class, rather add prefabs to the hierarchy of your scene. 
    /// and use attributes and methods from an instance of them in your script.</b> 
    /// Example:
    ///     - midiFilePlayer.MPTK_ChorusDelay = 0.2
    ///     - midiStreamPlayer.MPTK_InitSynth()
    /// </summary>
#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
    public partial class MidiSynth : MonoBehaviour, IMixerProcessor
    {
#else
    //[ExecuteAlways]
    public partial class MidiSynth : MonoBehaviour
    {
#endif
        //-------------------------------------------------------------------------
        //
        //              Attenuation on distance and Specialization
        // Distance Attenuation Parameters
        //
        // Maestro uses three parameters to control how an audio source is attenuated with distance.
        // These parameters map directly to Unity’s 3D audio behaviour but are presented in a simpler form.
        // MPTK_MinDistance
        //      Distance at which attenuation begins.
        //      When the listener is closer than this value, the audio plays at full volume(1.0).
        //
        //  MPTK_MaxDistance
        //      Distance at which attenuation reaches its minimum level.
        //      Beyond this distance, the volume remains constant and does not decrease further.
        //
        //  MPTK_MinSoundAttenuation
        //      Minimum volume applied at Max Distance.
        //      The value must be between 0.0 and 1.0.
        //-------------------------------------------------------------------------
        #region Attenation on distance and Spacialization


        /// @name Spatialization
        /// @{

        /// <summary>@brief
        /// If true, the MIDI player will be automatically paused 
        /// when the distance from the listener exceeds MPTK_MaxDistance.
        /// @version 2.16.1
        /// </summary>
        [HideInInspector] public bool MPTK_PauseOnMaxDistance = true;
        AnimationCurve customCurveAudioSource;
        [SerializeField][HideInInspector] private float minDistance, maxDistance;
        [SerializeField][HideInInspector] private float minSoundAttenuation;


        /// <summary>@brief 
        /// Should the Unity attenuation on distance effect must be enabled?\n
        /// See here how to setup attenuation with Unity https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Spatialization-Parameters
        /// if MPTK_DistanceAttenuation is true:\n
        ///     AudioSource.minDistance = MPTK_MinDistance;
        ///     AudioSource.maxDistance = MPTK_MaxDistance\n
        ///     AudioSource.spatialBlend = 1\n
        ///     AudioSource.spatialize = true\n
        ///     AudioSource.spatializePostEffects = true\n
        ///     AudioSource.rolloffMode = AudioRolloffMode.Custom;
        ///     AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customCurveAudioSource);
        ///     AudioSource.loop = true;
        ///     AudioSource.volume = 1f;
        /// @note: 
        ///     - <c>MPTK_Spatilize</c> has been renamed to  <c>MPTK_DistanceAttenuation</c> in v2.18.0.
        ///     - To be used also with MPTK_Orientation (Pro) to apply panning and filtering effects depending on the position of the sound source and the listener.
        /// </summary>
        [HideInInspector]
        public bool MPTK_DistanceAttenuation
        {
            get { return distanceAttenuation; }
            set
            {
                // Avoid call if no change
                if (distanceAttenuation != value /*|| !distanceAttenuationInitialized*/)
                {
                    //distanceAttenuationInitialized = true;
                    distanceAttenuation = value;
                    SetDistanceAttenuation();
                }
            }
        }


        /// <summary>@brief 
        /// When MPTK_DistanceAttenuation is enabled, the volume of the audio source depends on the distance between the audio source and the listener.
        /// Distance at which attenuation begins. When the listener is closer than this value, the audio plays at full volume(1.0). 
        /// </summary>
        [HideInInspector]
        public float MPTK_MinDistance
        {
            get
            {
                return minDistance;
            }
            set
            {
                try
                {
                    if (minDistance != value)
                    {
                        if (value < 0)
                            minDistance = 0;
                        else
                            minDistance = value;
                        if (value > maxDistance)
                            minDistance = maxDistance;
                        SetDistanceAttenuation();
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }


        /// <summary>@brief 
        /// When MPTK_DistanceAttenuation is enabled, the volume of the audio source depends on the distance between the audio source and the listener.
        /// Distance at which attenuation reaches its minimum level. Beyond this distance, the volume remains constant and does not decrease further
        /// </summary>
        [HideInInspector]
        public float MPTK_MaxDistance
        {
            get
            {
                return maxDistance;
            }
            set
            {
                try
                {
                    if (maxDistance != value)
                    {
                        if (value < 0)
                            maxDistance = 0;
                        else
                            maxDistance = value;
                        SetDistanceAttenuation();
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>@brief 
        /// If MPTK_DistanceAttenuation is enabled, the volume of the audio source depends on the distance between the audio source and the listener.
        /// Minimum volume applied at Max Distance
        /// </summary>
        [HideInInspector]
        public float MPTK_MinSoundAttenuation
        {
            get
            {
                return minSoundAttenuation;
            }
            set
            {
                try
                {
                    if (minSoundAttenuation != value)
                    {
                        minSoundAttenuation = value;
                        SetDistanceAttenuation();
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// @}

        protected void SetDistanceAttenuation()
        {
            if (MPTK_CorePlayer)
            {
                if (CoreAudioSource != null)
                {
                    ApplyDistanceAttenuationToAudioSource(CoreAudioSource);
#if MPTK_PRO
                    if (this is MidiSpatializer)
                        ApplyDistanceAttenuationForSpatializer();
#endif
                }
            }
            else
            {
                if (ActiveVoices != null)
                    for (int i = 0; i < ActiveVoices.Count; i++)
                    {
                        fluid_voice voice = ActiveVoices[i];
                        if (voice.VoiceAudio != null)
                        {
                            ApplyDistanceAttenuationToAudioSource(CoreAudioSource);
                        }
                    }
                if (FreeVoices != null)
                    for (int i = 0; i < FreeVoices.Count; i++)
                    {
                        fluid_voice voice = FreeVoices[i];
                        if (voice.VoiceAudio != null)
                        {
                            ApplyDistanceAttenuationToAudioSource(voice.VoiceAudio.Audiosource);
                        }
                    }
            }
        }
        private void ApplyDistanceAttenuationToAudioSource(AudioSource audioSource)
        {
            if (VerboseSpatialSynth)
                Debug.Log($"ApplySpatialToAudioSource Distance:{distanceAttenuation} MaxDistance:{maxDistance:F2} MinAttenuation:{MPTK_MinSoundAttenuation:F2} CorePlayer:{MPTK_CorePlayer} {CoreAudioSource?.name}");

            if (distanceAttenuation)
            {
                //if (customCurveAudioSource == null)
                customCurveAudioSource = new AnimationCurve(new Keyframe(minDistance, 1), new Keyframe(maxDistance, minSoundAttenuation));
                
                // Not working - remove 2.17.1
                //else
                //{
                //    Keyframe[] keys = customCurveAudioSource.keys;
                //    keys[0].time = minDistance;
                //    keys[1].time = maxDistance;
                //    keys[1].value = minSoundAttenuation;
                //    customCurveAudioSource.keys = keys;
                //}
                
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customCurveAudioSource);
                audioSource.minDistance = minDistance;
                audioSource.maxDistance = maxDistance;
                audioSource.spatialBlend = 1f;
                audioSource.spatialize = true;
                audioSource.spatializePostEffects = true;
                audioSource.loop = true;
                audioSource.volume = 1f;
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                audioSource.spatialBlend = 0f;
                audioSource.spatialize = false;
                audioSource.spatializePostEffects = false;
            }
        }
        // create a short empty clip
        private AudioClip CreateEmptyClip()
        {
            int sampleRate = 44100;
            int sampleCount = 10;
            int sampleChannel = 1;
            AudioClip myClip = AudioClip.Create("blank", sampleCount, sampleChannel, sampleRate, false);
            float[] samples = new float[sampleCount * sampleChannel];
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = 0f;
            }
            myClip.SetData(samples, 0);
            return myClip;
        }

        #endregion
    }
}
