using RoR2;
using RoR2.UI;
using UnityEngine;

namespace CrocsItems
{
    public class VFXUtils
    {
        public static void RecolorMaterialsAndLights(GameObject gameObject, Color32 primaryColor, Color32 emissionAndLightColor, bool convertRampsToGrayscale, bool emissionColorToPrimaryColor = false)
        {
            // for itself
            RecolorMaterialsAndLightsInternal(gameObject, primaryColor, emissionAndLightColor, convertRampsToGrayscale, emissionColorToPrimaryColor);

            // and for all children recursively
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                RecolorMaterialsAndLightsInternal(child.gameObject, primaryColor, emissionAndLightColor, convertRampsToGrayscale, emissionColorToPrimaryColor);
            }
        }

        private static void RecolorMaterialsAndLightsInternal(GameObject gameObject, Color32 primaryColor, Color32 emissionAndLightColor, bool convertRampsToGrayscale, bool emissionColorToPrimaryColor = false)
        {
            foreach (ParticleSystem particleSystem in gameObject.GetComponents<ParticleSystem>())
            {
                var particleSystemMain = particleSystem.main;
                var particleSystemStartColor = particleSystemMain.startColor;
                particleSystemStartColor.mode = ParticleSystemGradientMode.Color;
                particleSystemStartColor.color = primaryColor;

                var particleSystemColorOverLifetime = particleSystem.colorOverLifetime;
                if (particleSystemColorOverLifetime.color.gradient != null && particleSystemColorOverLifetime.color.gradient.colorKeys.Length > 0)
                {
                    var colorKeys = new GradientColorKey[2];
                    colorKeys[0] = new GradientColorKey(Color.white, 0f);
                    colorKeys[1] = new GradientColorKey(Color.white, 1f);
                    var alphaKeys = new GradientAlphaKey[2];
                    alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                    alphaKeys[0] = new GradientAlphaKey(0f, 1f);
                    particleSystemColorOverLifetime.color.gradient.SetKeys(colorKeys, alphaKeys);
                    // fuck off tbh
                }
            }

            foreach (Renderer renderer in gameObject.GetComponents<Renderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    switch (material.shader.name)
                    {
                        case "Hopoo Games/FX/Cloud Remap":
                        case "Hopoo Games/FX/Opaque Cloud Remap":
                        case "Hopoo Games/FX/Cloud Intersection Remap":
                            material.SetColor("_TintColor", primaryColor);
                            material.SetColor("_EmissionColor", emissionColorToPrimaryColor ? primaryColor : emissionAndLightColor);
                            if (convertRampsToGrayscale)
                            {
                                material.SetTexture("_RemapTex", Main.texRampTritone);
                                // guid is tex ramp tritone
                            }
                            break;

                        case "Hopoo Games/Deferred/Standard":
                            material.SetColor("_Color", primaryColor);
                            material.SetColor("_EmColor", emissionColorToPrimaryColor ? primaryColor : emissionAndLightColor);
                            break;
                    }
                }
            }

            foreach (LineRenderer lineRenderer in gameObject.GetComponents<LineRenderer>())
            {
                lineRenderer.startColor = primaryColor;
                lineRenderer.endColor = emissionAndLightColor;
            }

            foreach (Light light in gameObject.GetComponents<Light>())
            {
                light.color = emissionAndLightColor;
            }
        }

        public static void MultiplyDuration(GameObject gameObject, float durationMultiplier, float maximumDuration = -999f)
        {
            // for itself
            MultiplyDurationInternal(gameObject, durationMultiplier, maximumDuration);

            // and for all children recursively
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                MultiplyDurationInternal(child.gameObject, durationMultiplier, maximumDuration);
            }
        }

        private static void MultiplyDurationInternal(GameObject gameObject, float durationMultiplier, float maximumDuration = -999f)
        {
            foreach (ParticleSystem particleSystem in gameObject.GetComponents<ParticleSystem>())
            {
                var particleSystemMain = particleSystem.main;
                var particleSystemLifetime = particleSystemMain.startLifetime;
                float finalDuration = particleSystemLifetime.constant;
                if (particleSystemLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    finalDuration = (particleSystemLifetime.constantMin + particleSystemLifetime.constantMax) / 2f;
                }
                particleSystemLifetime.mode = ParticleSystemCurveMode.Constant;
                particleSystemLifetime.constant = finalDuration * durationMultiplier;
            }

            foreach (DestroyOnTimer destroyOnTimer in gameObject.GetComponents<DestroyOnTimer>())
            {
                destroyOnTimer.duration *= durationMultiplier;
            }

            foreach (AnimateShaderAlpha animateShaderAlpha in gameObject.GetComponents<AnimateShaderAlpha>())
            {
                animateShaderAlpha.timeMax *= durationMultiplier;
                if (animateShaderAlpha.GetComponent<DestroyOnTimer>() != null)
                {
                    animateShaderAlpha.timeMax = Mathf.Min(animateShaderAlpha.timeMax, animateShaderAlpha.GetComponent<DestroyOnTimer>().duration);
                }
                if (maximumDuration > 0f)
                {
                    animateShaderAlpha.timeMax = Mathf.Min(animateShaderAlpha.timeMax, maximumDuration);
                }
            }

            foreach (PostProcessDuration postProcessDuration in gameObject.GetComponents<PostProcessDuration>())
            {
                postProcessDuration.maxDuration *= durationMultiplier;
            }

            foreach (ObjectScaleCurve objectScaleCurve in gameObject.GetComponents<ObjectScaleCurve>())
            {
                objectScaleCurve.timeMax *= durationMultiplier;
            }

            foreach (AnimateUIAlpha animateUIAlpha in gameObject.GetComponents<AnimateUIAlpha>())
            {
                animateUIAlpha.timeMax *= durationMultiplier;
            }

            foreach (AnimateImageAlpha animateImageAlpha in gameObject.GetComponents<AnimateImageAlpha>())
            {
                animateImageAlpha.timeMax *= durationMultiplier;
            }

            foreach (LightIntensityCurve lightIntensityCurve in gameObject.GetComponents<LightIntensityCurve>())
            {
                lightIntensityCurve.timeMax *= durationMultiplier;
            }
        }

        public static Light AddLight(GameObject gameObject, Color32 color, float intensity, float range, float fadeOutTime)
        {
            // Main.ModLogger.LogError("gameobject is " + gameObject);
            var light = gameObject.AddComponent<Light>();
            // gameObject.GetComponent<Light>();
            // Main.ModLogger.LogError("light is " + light);
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;

            var lightIntensityCurve = gameObject.AddComponent<LightIntensityCurve>();
            lightIntensityCurve.timeMax = fadeOutTime;
            lightIntensityCurve.curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            lightIntensityCurve.light = light;

            return light;
        }

        public static Light AddLight(GameObject gameObject, Color32 color, float intensity, float range)
        {
            // Main.ModLogger.LogError("gameobject is " + gameObject);
            var light = gameObject.AddComponent<Light>();
            // gameObject.GetComponent<Light>();
            // Main.ModLogger.LogError("light is " + light);
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;

            return light;
        }

        public static void ScaleToHierarchy(GameObject gameObject)
        {
            ScaleToHierarchyInternal(gameObject);

            // and for all children recursively
            foreach (Transform child in gameObject.transform.GetComponentsInChildren<Transform>())
            {
                ScaleToHierarchyInternal(child.gameObject);
            }
        }

        private static void ScaleToHierarchyInternal(GameObject gameObject)
        {
            var particleSystem = gameObject.GetComponent<ParticleSystem>();
            if (!particleSystem)
            {
                return;
            }

            var main = particleSystem.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }
}