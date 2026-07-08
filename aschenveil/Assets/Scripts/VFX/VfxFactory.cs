using UnityEngine;
using UnityEngine.Rendering;

namespace Ashenveil.VFX
{
    /// <summary>
    /// Procedural ParticleSystem builders for runtime and editor scene setup.
    /// Referenced GDD section: Grafik-Stack.
    /// </summary>
    public static class VfxFactory
    {
        private const string ParticleShaderName = "Universal Render Pipeline/Particles/Unlit";
        private const string BaseColorProperty = "_BaseColor";
        private const string SurfaceProperty = "_Surface";
        private const string BlendProperty = "_Blend";
        private const string SrcBlendProperty = "_SrcBlend";
        private const string DstBlendProperty = "_DstBlend";
        private const string SrcBlendAlphaProperty = "_SrcBlendAlpha";
        private const string DstBlendAlphaProperty = "_DstBlendAlpha";
        private const string ZWriteProperty = "_ZWrite";
        private const string SoftParticlesProperty = "_SoftParticlesEnabled";
        private const string SoftParticlesKeyword = "_SOFTPARTICLES_ON";
        private const string TransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";

        private const float TransparentSurface = 1f;
        private const float AlphaBlendMode = 0f;
        private const float AdditiveBlendMode = 2f;
        private const int TransparentRenderQueue = 3000;

        /// <summary>
        /// Builds a low-rate cyan shimmer of upward drifting motes under the given parent.
        /// </summary>
        /// <param name="parent">Transform that owns the particle GameObject.</param>
        /// <param name="color">Particle tint. A transparent color falls back to cyan.</param>
        /// <returns>A new GameObject containing the configured ParticleSystem.</returns>
        public static GameObject BuildAetherShimmer(Transform parent, Color color)
        {
            Color tint = ResolveColor(color, ParticleBudget.AetherDefaultColor);
            ParticleSystem particles = CreateParticleSystem("Aether Shimmer VFX", parent, tint, true, true);

            ConfigureMain(
                particles,
                true,
                ParticleBudget.AetherShimmerLifetimeMin,
                ParticleBudget.AetherShimmerLifetimeMax,
                ParticleBudget.AetherShimmerSizeMin,
                ParticleBudget.AetherShimmerSizeMax,
                ParticleSystemSimulationSpace.Local);
            ConfigureEmission(particles, ParticleBudget.AetherShimmerRate);
            ConfigureSphereShape(particles, ParticleBudget.AetherShimmerRadius);
            ConfigureUpwardVelocity(
                particles,
                ParticleBudget.AetherShimmerVelocityMin,
                ParticleBudget.AetherShimmerVelocityMax);
            ConfigureColorOverLifetime(
                particles,
                WithAlpha(tint, ParticleBudget.InvisibleAlpha),
                WithAlpha(tint, ParticleBudget.SoftAetherAlpha),
                WithAlpha(tint, ParticleBudget.InvisibleAlpha));
            ConfigureSizeOverLifetime(
                particles,
                SizeCurve(
                    ParticleBudget.AetherShimmerSizeStart,
                    ParticleBudget.AetherShimmerSizeMiddle,
                    ParticleBudget.AetherShimmerSizeEnd));

            return particles.gameObject;
        }

        /// <summary>
        /// Builds a compact hand glow emitter with tiny aether sparks.
        /// </summary>
        /// <param name="parent">Transform that owns the particle GameObject.</param>
        /// <param name="color">Particle tint. A transparent color falls back to cyan.</param>
        /// <returns>A new GameObject containing the configured ParticleSystem.</returns>
        public static GameObject BuildHandGlow(Transform parent, Color color)
        {
            Color tint = ResolveColor(color, ParticleBudget.HandGlowDefaultColor);
            ParticleSystem particles = CreateParticleSystem("Hand Glow VFX", parent, tint, true, false);

            ConfigureMain(
                particles,
                true,
                ParticleBudget.HandGlowLifetimeMin,
                ParticleBudget.HandGlowLifetimeMax,
                ParticleBudget.HandGlowSizeMin,
                ParticleBudget.HandGlowSizeMax,
                ParticleSystemSimulationSpace.Local);
            ConfigureEmission(particles, ParticleBudget.HandGlowRate);
            ConfigureSphereShape(particles, ParticleBudget.HandGlowRadius);
            ConfigureUpwardVelocity(
                particles,
                ParticleBudget.HandGlowVelocityMin,
                ParticleBudget.HandGlowVelocityMax);
            ConfigureColorOverLifetime(
                particles,
                WithAlpha(tint, ParticleBudget.InvisibleAlpha),
                WithAlpha(tint, ParticleBudget.StrongAetherAlpha),
                WithAlpha(tint, ParticleBudget.InvisibleAlpha));
            ConfigureSizeOverLifetime(
                particles,
                SizeCurve(
                    ParticleBudget.HandGlowSizeStart,
                    ParticleBudget.HandGlowSizeMiddle,
                    ParticleBudget.HandGlowSizeEnd));

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles.gameObject;
        }

        /// <summary>
        /// Builds an orange/yellow cone flame effect under the given parent.
        /// </summary>
        /// <param name="parent">Transform that owns the particle GameObject.</param>
        /// <returns>A new GameObject containing the configured ParticleSystem.</returns>
        public static GameObject BuildFire(Transform parent)
        {
            ParticleSystem particles = CreateParticleSystem("Fire VFX", parent, ParticleBudget.FireStartColor, true, true);

            ConfigureMain(
                particles,
                true,
                ParticleBudget.FireLifetimeMin,
                ParticleBudget.FireLifetimeMax,
                ParticleBudget.FireSizeMin,
                ParticleBudget.FireSizeMax,
                ParticleSystemSimulationSpace.Local);
            ConfigureEmission(particles, ParticleBudget.FireRate);
            ConfigureConeShape(particles, ParticleBudget.FireConeRadius, ParticleBudget.FireConeAngle);
            ConfigureUpwardVelocity(particles, ParticleBudget.FireVelocityMin, ParticleBudget.FireVelocityMax);
            ConfigureColorOverLifetime(
                particles,
                ParticleBudget.FireStartColor,
                ParticleBudget.FireMiddleColor,
                ParticleBudget.FireEndColor);
            ConfigureSizeOverLifetime(
                particles,
                SizeCurve(
                    ParticleBudget.FireSizeStart,
                    ParticleBudget.FireSizeMiddle,
                    ParticleBudget.FireSizeEnd));

            return particles.gameObject;
        }

        /// <summary>
        /// Builds a dark grey smoke column that rises, expands, and fades.
        /// </summary>
        /// <param name="parent">Transform that owns the particle GameObject.</param>
        /// <returns>A new GameObject containing the configured ParticleSystem.</returns>
        public static GameObject BuildSmoke(Transform parent)
        {
            ParticleSystem particles = CreateParticleSystem("Smoke VFX", parent, ParticleBudget.SmokeStartColor, false, true);

            ConfigureMain(
                particles,
                true,
                ParticleBudget.SmokeLifetimeMin,
                ParticleBudget.SmokeLifetimeMax,
                ParticleBudget.SmokeSizeMin,
                ParticleBudget.SmokeSizeMax,
                ParticleSystemSimulationSpace.Local);
            ConfigureEmission(particles, ParticleBudget.SmokeRate);
            ConfigureConeShape(particles, ParticleBudget.SmokeConeRadius, ParticleBudget.SmokeConeAngle);
            ConfigureUpwardVelocity(particles, ParticleBudget.SmokeVelocityMin, ParticleBudget.SmokeVelocityMax);
            ConfigureColorOverLifetime(
                particles,
                ParticleBudget.SmokeStartColor,
                ParticleBudget.SmokeMiddleColor,
                ParticleBudget.SmokeEndColor);
            ConfigureSizeOverLifetime(
                particles,
                SizeCurve(
                    ParticleBudget.SmokeSizeStart,
                    ParticleBudget.SmokeSizeMiddle,
                    ParticleBudget.SmokeSizeEnd));

            return particles.gameObject;
        }

        /// <summary>
        /// Builds sparse orange embers that rise above a burning object.
        /// </summary>
        /// <param name="parent">Transform that owns the particle GameObject.</param>
        /// <returns>A new GameObject containing the configured ParticleSystem.</returns>
        public static GameObject BuildEmbers(Transform parent)
        {
            ParticleSystem particles = CreateParticleSystem("Embers VFX", parent, ParticleBudget.EmberStartColor, true, true);

            ConfigureMain(
                particles,
                true,
                ParticleBudget.EmberLifetimeMin,
                ParticleBudget.EmberLifetimeMax,
                ParticleBudget.EmberSizeMin,
                ParticleBudget.EmberSizeMax,
                ParticleSystemSimulationSpace.Local);
            ConfigureEmission(particles, ParticleBudget.EmberRate);
            ConfigureSphereShape(particles, ParticleBudget.EmberRadius);
            ConfigureUpwardVelocity(particles, ParticleBudget.EmberVelocityMin, ParticleBudget.EmberVelocityMax);
            ConfigureColorOverLifetime(
                particles,
                ParticleBudget.EmberStartColor,
                ParticleBudget.EmberMiddleColor,
                ParticleBudget.EmberEndColor);
            ConfigureSizeOverLifetime(
                particles,
                SizeCurve(
                    ParticleBudget.EmberSizeStart,
                    ParticleBudget.EmberSizeMiddle,
                    ParticleBudget.EmberSizeEnd));

            return particles.gameObject;
        }

        private static ParticleSystem CreateParticleSystem(
            string name,
            Transform parent,
            Color materialColor,
            bool additive,
            bool playOnAwake)
        {
            GameObject gameObject = new GameObject(name);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            ParticleSystem particles = gameObject.AddComponent<ParticleSystem>();
            if (!gameObject.TryGetComponent(out ParticleSystemRenderer renderer))
            {
                renderer = gameObject.AddComponent<ParticleSystemRenderer>();
            }

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateParticleMaterial(materialColor, additive);

            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = playOnAwake;
            main.loop = true;

            return particles;
        }

        private static Material CreateParticleMaterial(Color color, bool additive)
        {
            Shader shader = Shader.Find(ParticleShaderName);
            Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
            material.name = additive ? "Procedural Additive Particle Material" : "Procedural Alpha Particle Material";
            material.SetColor(BaseColorProperty, color);
            material.SetFloat(SurfaceProperty, TransparentSurface);
            material.SetFloat(BlendProperty, additive ? AdditiveBlendMode : AlphaBlendMode);
            material.SetFloat(SrcBlendProperty, additive ? (float)BlendMode.SrcAlpha : (float)BlendMode.SrcAlpha);
            material.SetFloat(DstBlendProperty, additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat(SrcBlendAlphaProperty, (float)BlendMode.One);
            material.SetFloat(DstBlendAlphaProperty, additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat(ZWriteProperty, ParticleBudget.DisabledFloat);
            material.SetFloat(SoftParticlesProperty, ParticleBudget.EnabledFloat);
            material.EnableKeyword(TransparentKeyword);
            material.EnableKeyword(SoftParticlesKeyword);
            material.renderQueue = TransparentRenderQueue;

            return material;
        }

        private static void ConfigureMain(
            ParticleSystem particles,
            bool loop,
            float lifetimeMin,
            float lifetimeMax,
            float sizeMin,
            float sizeMax,
            ParticleSystemSimulationSpace simulationSpace)
        {
            ParticleSystem.MainModule main = particles.main;
            main.loop = loop;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startSpeed = ParticleBudget.DisabledFloat;
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            main.simulationSpace = simulationSpace;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = ParticleBudget.MaxParticlesPerEffect;
        }

        private static void ConfigureEmission(ParticleSystem particles, float rateOverTime)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(rateOverTime);
        }

        private static void ConfigureSphereShape(ParticleSystem particles, float radius)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.radiusThickness = ParticleBudget.ShapeRadiusThickness;
        }

        private static void ConfigureConeShape(ParticleSystem particles, float radius, float angle)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.radius = radius;
            shape.angle = angle;
            shape.radiusThickness = ParticleBudget.ShapeRadiusThickness;
        }

        private static void ConfigureUpwardVelocity(ParticleSystem particles, float yMin, float yMax)
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-yMin, yMin);
            velocity.y = new ParticleSystem.MinMaxCurve(yMin, yMax);
            velocity.z = new ParticleSystem.MinMaxCurve(-yMin, yMin);
        }

        private static void ConfigureColorOverLifetime(
            ParticleSystem particles,
            Color start,
            Color middle,
            Color end)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(start, ParticleBudget.StartTime),
                    new GradientColorKey(middle, ParticleBudget.MiddleTime),
                    new GradientColorKey(end, ParticleBudget.EndTime)
                },
                new[]
                {
                    new GradientAlphaKey(start.a, ParticleBudget.StartTime),
                    new GradientAlphaKey(middle.a, ParticleBudget.MiddleTime),
                    new GradientAlphaKey(end.a, ParticleBudget.EndTime)
                });

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static void ConfigureSizeOverLifetime(ParticleSystem particles, AnimationCurve curve)
        {
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(ParticleBudget.FullSize, curve);
        }

        private static AnimationCurve SizeCurve(float start, float middle, float end)
        {
            return new AnimationCurve(
                new Keyframe(ParticleBudget.StartTime, start),
                new Keyframe(ParticleBudget.MiddleTime, middle),
                new Keyframe(ParticleBudget.EndTime, end));
        }

        private static Color ResolveColor(Color color, Color fallback)
        {
            return color.a > ParticleBudget.InvisibleAlpha ? color : fallback;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
