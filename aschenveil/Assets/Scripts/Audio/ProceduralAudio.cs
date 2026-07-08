using UnityEngine;

namespace Ashenveil.Audio
{
    public static class ProceduralAudio
    {
        public const int SampleRate = 44100;

        private const float TwoPi = Mathf.PI * 2f;

        public static AudioClip Wind(float seconds)
        {
            int sampleCount = SecondsToSamples(seconds);
            float[] samples = new float[sampleCount];
            NoiseState noise = new NoiseState(3101);
            float drift = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / SampleRate;
                drift = Mathf.Lerp(drift, noise.NextBrown(), 0.004f);
                float gust = 0.65f + 0.35f * Mathf.Sin(TwoPi * 0.08f * time + 0.8f);
                float leafNoise = noise.NextPinkish() * 0.18f;
                samples[i] = (drift * 0.42f + leafNoise) * gust;
            }

            BlendLoopSeam(samples);
            Normalize(samples, 0.65f);
            return CreateClip("Procedural Wind", samples);
        }

        public static AudioClip SwordSwing()
        {
            int sampleCount = SecondsToSamples(0.25f);
            float[] samples = new float[sampleCount];
            NoiseState noise = new NoiseState(4103);
            float filtered = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float progress = (float)i / (samples.Length - 1);
                float attack = Mathf.Clamp01(progress / 0.08f);
                float decay = Mathf.Pow(1f - progress, 2.8f);
                float sweep = Mathf.Sin(TwoPi * Mathf.Lerp(170f, 70f, progress) * i / SampleRate);

                filtered = Mathf.Lerp(filtered, noise.NextPinkish(), 0.22f);
                samples[i] = (filtered * 0.78f + sweep * 0.12f) * attack * decay;
            }

            Normalize(samples, 0.82f);
            return CreateClip("Procedural Sword Swing", samples);
        }

        public static AudioClip Hit()
        {
            int sampleCount = SecondsToSamples(0.15f);
            float[] samples = new float[sampleCount];
            NoiseState noise = new NoiseState(5107);
            float body = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / SampleRate;
                float progress = (float)i / (samples.Length - 1);
                float punch = Mathf.Exp(-progress * 20f);
                float tail = Mathf.Exp(-progress * 8f);

                body = Mathf.Lerp(body, noise.NextPinkish(), 0.35f);
                float thud = Mathf.Sin(TwoPi * 82f * time) * tail;
                samples[i] = body * 0.55f * punch + thud * 0.5f;
            }

            Normalize(samples, 0.9f);
            return CreateClip("Procedural Hit", samples);
        }

        public static AudioClip Ui()
        {
            int sampleCount = SecondsToSamples(0.06f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / SampleRate;
                float progress = (float)i / (samples.Length - 1);
                float envelope = Mathf.Clamp01(progress / 0.08f) * Mathf.Exp(-progress * 18f);
                float tone = Mathf.Sin(TwoPi * 880f * time) + Mathf.Sin(TwoPi * 1320f * time) * 0.35f;
                samples[i] = tone * envelope * 0.38f;
            }

            Normalize(samples, 0.55f);
            return CreateClip("Procedural UI", samples);
        }

        public static AudioClip AetherHum(float seconds)
        {
            int sampleCount = SecondsToSamples(seconds);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / SampleRate;
                float tremolo = 0.72f + 0.28f * Mathf.Sin(TwoPi * 0.34f * time);
                float root = Mathf.Sin(TwoPi * 110f * time);
                float fifth = Mathf.Sin(TwoPi * 165f * time + 0.25f);
                float shimmer = Mathf.Sin(TwoPi * 330f * time + Mathf.Sin(TwoPi * 0.08f * time)) * 0.12f;

                samples[i] = (root * 0.44f + fifth * 0.32f + shimmer) * tremolo;
            }

            BlendLoopSeam(samples);
            Normalize(samples, 0.6f);
            return CreateClip("Procedural Aether Hum", samples);
        }

        public static AudioClip Fire(float seconds)
        {
            int sampleCount = SecondsToSamples(seconds);
            float[] samples = new float[sampleCount];
            NoiseState noise = new NoiseState(6109);
            float bed = 0f;
            float crackle = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float random = noise.NextWhite01();
                bed = Mathf.Lerp(bed, noise.NextPinkish(), 0.08f);

                if (random > 0.988f)
                {
                    crackle += (random - 0.988f) * 45f;
                }

                crackle *= 0.78f;
                samples[i] = bed * 0.16f + crackle * noise.NextPinkish() * 0.42f;
            }

            BlendLoopSeam(samples);
            Normalize(samples, 0.58f);
            return CreateClip("Procedural Fire", samples);
        }

        private static AudioClip CreateClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static int SecondsToSamples(float seconds)
        {
            return Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(0.01f, seconds) * SampleRate));
        }

        private static void Normalize(float[] samples, float targetPeak)
        {
            float peak = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
            }

            if (peak <= 0f)
            {
                return;
            }

            float scale = targetPeak / peak;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scale;
            }
        }

        private static void BlendLoopSeam(float[] samples)
        {
            int fadeSamples = Mathf.Min(2048, samples.Length / 8);
            if (fadeSamples <= 1)
            {
                return;
            }

            int tailStart = samples.Length - fadeSamples;
            for (int i = 0; i < fadeSamples; i++)
            {
                float blend = (float)i / (fadeSamples - 1);
                float head = samples[i];
                float tail = samples[tailStart + i];

                samples[tailStart + i] = Mathf.Lerp(tail, head, blend);
            }
        }

        private struct NoiseState
        {
            private readonly System.Random _random;
            private float _brown;
            private float _pink0;
            private float _pink1;
            private float _pink2;

            public NoiseState(int seed)
            {
                _random = new System.Random(seed);
                _brown = 0f;
                _pink0 = 0f;
                _pink1 = 0f;
                _pink2 = 0f;
            }

            public float NextWhite01()
            {
                return (float)_random.NextDouble();
            }

            public float NextBrown()
            {
                _brown += NextWhite() * 0.035f;
                _brown = Mathf.Clamp(_brown, -1f, 1f);
                _brown *= 0.995f;
                return _brown;
            }

            public float NextPinkish()
            {
                float white = NextWhite();
                _pink0 = 0.99765f * _pink0 + white * 0.099046f;
                _pink1 = 0.963f * _pink1 + white * 0.296516f;
                _pink2 = 0.57f * _pink2 + white * 1.052691f;
                return (_pink0 + _pink1 + _pink2 + white * 0.1848f) * 0.18f;
            }

            private float NextWhite()
            {
                return (float)(_random.NextDouble() * 2.0 - 1.0);
            }
        }
    }
}
