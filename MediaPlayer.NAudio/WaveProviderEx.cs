﻿using System;
using NAudio.Wave;

namespace MediaPlayer.NAudio
{
    internal class WaveProviderEx : WaveChannel32
    {
        public WaveProviderEx(Uri source) : base(new AudioStream(source))
        {
        }
    }
}