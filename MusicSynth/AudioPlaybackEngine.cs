using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGame.MusicSynth
{
    public class AudioUtils
    {
        public static double NoteToFrequency(int noteNumber)
        {
            // 将MIDI音符号转换为频率
            return 440.0 * Math.Pow(2, (noteNumber - 69) / 12.0);
        }

        public static double NoteToFrequency(int noteNumber, double detuneCents)
        {
            // 将MIDI音符号和微调转换为频率
            return 440.0 * Math.Pow(2, (noteNumber - 69 + detuneCents / 100.0) / 12.0);
        }
    }

    // 音频播放引擎类，实现IDisposable接口
    class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice; // 音频输出设备
        private readonly MixingSampleProvider mixer; // 混音器

        // 构造函数，初始化输出设备和混音器
        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent(); // 创建WaveOutEvent实例作为输出设备
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true; // 设置混音器读取完全标志
            outputDevice.Init(mixer); // 初始化输出设备
            outputDevice.Play(); // 开始播放
        }

        // 播放音频文件
        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName); // 读取音频文件
            AddMixerInput(new AutoDisposeFileReader(input)); // 将音频输入添加到混音器
        }

        // 播放缓存音频
        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound)); // 将缓存音频输入添加到混音器
        }

        // 播放合成器生成的音频
        public void PlaySynth(Synth synth)
        {
            //AddMixerInput(synth); // 将合成器输入添加到混音器
        }

        // 转换音频通道数
        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input; // 如果通道数一致，直接返回输入
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input); // 单声道转立体声
            }
            throw new NotImplementedException("未实现此通道数转换");
        }

        // 添加混音器输入
        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input)); // 转换通道数后添加到混音器
        }

        // 释放资源
        public void Dispose()
        {
            outputDevice.Dispose(); // 释放输出设备资源
        }

        // 静态实例，单例模式
        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }

    // 缓存音频类
    internal class CachedSound
    {
        public float[] AudioData { get; private set; } // 音频数据
        public WaveFormat WaveFormat { get; private set; } // 音频格式

        // 构造函数，读取音频文件并缓存音频数据
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                WaveFormat = audioFileReader.WaveFormat; // 获取音频格式
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead)); // 读取音频数据
                }
                AudioData = wholeFile.ToArray(); // 转换为数组
            }
        }
    }

    // 缓存MIDI类
    internal class CachedMidi
    {
        public MidiFile MidiData { get; private set; } // MIDI数据

        // 构造函数，读取MIDI文件并缓存MIDI数据
        public CachedMidi(string midiFileName)
        {
            MidiData = new MidiFile(midiFileName); // 读取MIDI文件
        }
    }

    // 缓存音频采样提供者
    internal class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position; // 当前读取位置

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy); // 复制音频数据
            position += samplesToCopy; // 更新位置
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }

    // 自动释放文件读取器
    internal class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;

        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count); // 读取音频数据
            if (read == 0)
            {
                reader.Dispose(); // 读取完毕后释放资源
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
