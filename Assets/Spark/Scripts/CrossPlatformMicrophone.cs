//#if UNITY_WEBGL && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class CrossPlatformMicrophone
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void MicrophoneWebGL_Start(string deviceName, bool loop, int lengthSec, int frequency, int channelCount);

        [DllImport("__Internal")]
        private static extern int MicrophoneWebGL_GetPosition(string deviceName);

        [DllImport("__Internal")]
        private static extern void MicrophoneWebGL_End(string deviceName);

        [DllImport("__Internal")]
        private static extern void MicrophoneWebGL_GetData(string deviceName, float[] samples, int length, int offset);
#endif
        private static AudioClip _clip;

        public static void Start(string deviceName, bool loop, int lengthSec, int frequency)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            {
                MicrophoneWebGL_Start(deviceName, loop, lengthSec, frequency, 1);
            }
            #else
            {
                _clip = Microphone.Start(deviceName, loop, lengthSec, frequency);
            }
            #endif
        }

        public static void GetData(string deviceName, float[] data, int offsetSamples)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MicrophoneWebGL_GetData(deviceName, data, data.Length, offsetSamples);
#else
            _clip.GetData(data, offsetSamples);
#endif
        }

        public static int GetRecordingPosition(string device)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            {
                return MicrophoneWebGL_GetPosition(device);
            }
            #else
            {
                return Microphone.GetPosition(device);
            }
            #endif
        }

        public static void End(string device)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            {
                MicrophoneWebGL_End(device);
            }
            #else
            {
                Microphone.End(device);
            }
            #endif
        }
    }
}