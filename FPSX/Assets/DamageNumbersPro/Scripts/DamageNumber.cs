using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DamageNumbersPro
{
    public class DamageNumber : MonoBehaviour
    {
        //Lifetime:
        [Tooltip("The lifetime after which this fades out.")]
        public float lifetime = 2f;

        //Number:
        public bool enableNumber = true;
        [Tooltip("The number displayed in the text.\nCan be disabled if you only need text.")]
        public float number = 1;
        public TextSettings numberSettings = new TextSettings(0);
        public DigitSettings digitSettings = new DigitSettings(0);

        //Prefix:
        public bool enablePrefix = true;
        [Tooltip("Text displayed to the left of the number.")]
        public string prefix = "";
        public TextSettings prefixSettings = new TextSettings(0.2f);

        //Suffix:
        public bool enableSuffix = true;
        [Tooltip("Text displayed to the right of the number.")]
        public string suffix = "";
        public TextSettings suffixSettings = new TextSettings(0.2f);

        //Fading:
        public bool enableFading = true;
        public FadeSettings fadeIn = new FadeSettings(new Vector2(2,2));
        public FadeSettings fadeOut = new FadeSettings(new Vector2(1, 1));

        //Moving:
        public bool enableMovement = true;
        [Tooltip("The type of movement.")]
        public MoveType moveType = MoveType.LERP;
        public LerpSettings lerpSettings = new LerpSettings(0);
        public VelocitySettings velocitySettings = new VelocitySettings(0);

        //Start Rotation:
        public bool enableStartRotation = true;
        [Tooltip("Minimum start rotation.")]
        public float minRotation = -2f;
        [Tooltip("Maximum start rotation.")]
        public float maxRotation = 2f;

        //Shaking:
        public bool enableShaking = true;
        [Tooltip("Shake settings during idle.")]
        public ShakeSettings idleShake = new ShakeSettings(new Vector2(0.005f,0.005f));
        [Tooltip("Shake settings while fading in.\nCan be used to add motion while fading.")]
        public ShakeSettings fadeInShake = new ShakeSettings(Vector2.zero);
        [Tooltip("Shake settings while fading out.\nCan be used to add motion while fading.")]
        public ShakeSettings fadeOutShake = new ShakeSettings(Vector2.zero);

        //Combination:
        public bool enableCombination = false;
        public CombinationSettings combinationSettings = new CombinationSettings(0f);

        //Following:
        public bool enableFollowing = true;
        [Tooltip("Transform that will be followed.\nTries to maintain the position relative to the target.")]
        public Transform followedTarget;
        [Tooltip("Speed at which target is followed.")]
        public float followSpeed = 10f;
        public float followDrag = 0f;

        //Perspective Camera:
        public bool enablePerspective = true;
        [Tooltip("Keeps the numbers size consistent accross different distances.")]
        public bool consistentScale = true;
        public PerspectiveSettings perspectiveSettings = new PerspectiveSettings(0);
        [Tooltip("Override the camera looked at and scaled for.\nIf this set to None the Main Camera will be used.")]
        public Transform cameraOverride;

        //References:
        TextMeshPro textA;
        TextMeshPro textB;

        //Fading:
        float currentFade;
        float startTime;
        float startLifeTime;
        float baseAlpha;

        //Transform:
        public Vector3 position;

        //Scaling:
        Vector3 baseScale;
        Vector3 currentScale;

        //Movement:
        Vector2 remainingOffset;
        Vector2 currentVelocity;

        //Following:
        Vector3 lastTargetPosition;
        Vector3 targetOffset;
        float currentFollowSpeed;

        //Combination:
        static Dictionary<string, HashSet<DamageNumber>> combinationDictionary;
        DamageNumber myAbsorber;
        DamageNumber myTarget;
        bool removedFromDictionary;
        bool givenNumber;
        float absorbProgress;

        void Start()
        {
            //Init:
            GetReferences();
            InitVariables();
            TryCombination();

            //Destroy if lifetime <= 0:
            if(lifetime <= 0)
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            HandleFading();
            HandleMovement();
            HandleCombination();
        }

        void LateUpdate()
        {
            HandleFollowing();
            ApplyTransform();
        }

        /// <summary>
        /// Use this function on prefabs to spawn new damage numbers.
        /// Will clone this damage number.
        /// </summary>
        /// <returns></returns>
        public DamageNumber CreateNew(float newNumber, Vector3 newPosition)
        {
            //Create new number gameobject:
            GameObject newGO = Instantiate<GameObject>(gameObject);
            newGO.SetActive(true);

            //Get References:
            DamageNumber newDN = newGO.GetComponent<DamageNumber>();

            //Position
            newDN.transform.position = newPosition;

            //Number:
            newDN.number = newNumber;

            return newDN;
        }

        void HandleFollowing()
        {
            if (!enableFollowing || followedTarget == null) {
                lastTargetPosition = Vector3.zero;
                return;
            }

            //Get Offset:
            if(lastTargetPosition != Vector3.zero)
            {
                targetOffset += followedTarget.position - lastTargetPosition;
            }
            lastTargetPosition = followedTarget.position;

            if (followDrag > 0 && currentFollowSpeed > 0)
            {
                currentFollowSpeed -= followDrag * Time.deltaTime;

                if(currentFollowSpeed < 0)
                {
                    currentFollowSpeed = 0;
                }
            }

            //Move to Target:
            Vector3 oldOffset = targetOffset;
            targetOffset = Vector3.Lerp(targetOffset, Vector3.zero, Time.deltaTime * followSpeed * currentFollowSpeed);
            position += oldOffset - targetOffset;
        }

        void HandleCombination()
        {
            if (!enableCombination || Time.time - startTime < combinationSettings.delay) return;

            if(myAbsorber != null)
            {
                //Reset Lifetime:
                startLifeTime = Time.time;

                //Move:
                position = Vector3.Lerp(position, myAbsorber.position, Time.deltaTime * combinationSettings.targetSpeed);

                //Scale:
                baseScale = Vector3.Lerp(baseScale, Vector3.zero, Time.deltaTime * combinationSettings.targetScaleDownSpeed);

                //Fading:
                absorbProgress += Time.deltaTime;
                float normalizedProgress = absorbProgress / Mathf.Max(0.001f,combinationSettings.absorbTime);

                baseAlpha = Mathf.Min(baseAlpha, Mathf.Max(0,1 - normalizedProgress * combinationSettings.targetFadeOutSpeed));
                UpdateAlpha(currentFade);

                if (normalizedProgress >= 1)
                {
                    GiveNumber();
                    Destroy(gameObject);
                }
            }else if(myTarget != null)
            {
                //Move:
                position = Vector3.Lerp(position, myTarget.position, Time.deltaTime * combinationSettings.absorberSpeed);
            }
        }

        void HandleMovement()
        {
            if (enableMovement == false) return; //No Movement.

            if(moveType == MoveType.LERP)
            {
                //Lerp:
                Vector2 oldOffset = remainingOffset;
                remainingOffset = Vector2.Lerp(remainingOffset, Vector2.zero, Time.deltaTime * lerpSettings.speed);
                Vector2 deltaOffset = oldOffset - remainingOffset;

                position += transform.up * deltaOffset.y + transform.right * deltaOffset.x;
            }
            else
            {
                //Velocity:
                if (velocitySettings.dragX > 0)
                {
                    currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, Time.deltaTime * velocitySettings.dragX);
                }
                if (velocitySettings.dragY > 0)
                {
                    currentVelocity.y = Mathf.Lerp(currentVelocity.y, 0, Time.deltaTime * velocitySettings.dragY);
                }

                currentVelocity.y -= velocitySettings.gravity * Time.deltaTime;
                position += (transform.up * currentVelocity.y + transform.right * currentVelocity.x) * Time.deltaTime;
            }
        }

        void ApplyTransform()
        {
            //Position:
            Vector3 finalPosition = position;

            //Shaking:
            #region Shaking
            if (enableShaking)
            {
                Vector3 idleShakePosition = ApplyShake(finalPosition, idleShake);

                if (IsAlive())
                {
                    finalPosition = Vector3.Lerp(ApplyShake(finalPosition, fadeInShake), idleShakePosition, currentFade);
                }
                else
                {
                    finalPosition = Vector3.Lerp(ApplyShake(finalPosition, fadeOutShake), idleShakePosition, currentFade);
                }
            }
            #endregion

            transform.position = finalPosition;

            //Scale Down from Combination:
            if (enableCombination)
            {
                currentScale = Vector3.Lerp(currentScale, baseScale, Time.deltaTime * combinationSettings.absorberScaleFade);
            }

            Vector3 appliedScale = currentScale;

            //Perspective:
            Transform targetCamera = cameraOverride;
            if(targetCamera == null && Camera.main != null && Camera.main.orthographic == false)
            {
                targetCamera = Camera.main.transform;
            }
            if (enablePerspective && targetCamera != null)
            {
                transform.LookAt(targetCamera);
                transform.eulerAngles = new Vector3(-transform.eulerAngles.x, transform.eulerAngles.y + 180, 0);

                if (consistentScale)
                {
                    float camDistance = Vector3.Distance(transform.position, targetCamera.position);

                    appliedScale *= camDistance / Mathf.Max(1, perspectiveSettings.baseDistance);

                    if (camDistance < perspectiveSettings.closeDistance)
                    {
                        appliedScale *= perspectiveSettings.closeScale;
                    }
                    else if (camDistance > perspectiveSettings.farDistance)
                    {
                        appliedScale *= perspectiveSettings.farScale;
                    }
                    else
                    {
                        appliedScale *= perspectiveSettings.farScale + (perspectiveSettings.closeScale - perspectiveSettings.farScale) * Mathf.Clamp01(1 - (camDistance - perspectiveSettings.closeScale) / Mathf.Max(0.01f, perspectiveSettings.farDistance - perspectiveSettings.closeScale));
                    }
                }
            }

            transform.localScale = appliedScale;
        }

        void TryCombination()
        {
            if (enableCombination == false || combinationSettings.combinationGroup == "") return; //No Combination

            removedFromDictionary = false;
            givenNumber = false;
            absorbProgress = 0;

            //Create Dictionary:
            if (combinationDictionary == null)
            {
                combinationDictionary = new Dictionary<string, HashSet<DamageNumber>>();
            }

            //Create HashSet:
            if (!combinationDictionary.ContainsKey(combinationSettings.combinationGroup))
            {
                combinationDictionary.Add(combinationSettings.combinationGroup, new HashSet<DamageNumber>());
            }

            //Add to HashSet:
            combinationDictionary[combinationSettings.combinationGroup].Add(this);

            //Combination:
            if (combinationSettings.absorberType == AbsorberType.OLDEST)
            {
                float oldestStartTime = Time.time + 0.5f;
                DamageNumber oldestNumber = null;

                foreach(DamageNumber otherNumber in combinationDictionary[combinationSettings.combinationGroup])
                {
                    if(otherNumber != this && otherNumber.myAbsorber == null && otherNumber.startTime < oldestStartTime)
                    {
                        if (Vector3.Distance(otherNumber.position, position) < combinationSettings.maxDistance)
                        {
                            oldestStartTime = otherNumber.startTime;
                            oldestNumber = otherNumber;
                        }
                    }
                }

                if (oldestNumber != null)
                {
                    GetAbsorbed(oldestNumber);
                }
            }
            else
            {
                foreach (DamageNumber otherNumber in combinationDictionary[combinationSettings.combinationGroup])
                {
                    if (otherNumber != this)
                    {
                        if (Vector3.Distance(otherNumber.position, position) < combinationSettings.maxDistance)
                        {
                            if(otherNumber.myAbsorber == null)
                            {
                                otherNumber.startTime = Time.time - 0.01f;
                            }

                            otherNumber.GetAbsorbed(this);
                        }
                    }
                }
            }
        }
        public void GetAbsorbed(DamageNumber otherNumber)
        {
            otherNumber.myTarget = this;
            myAbsorber = otherNumber;
            myAbsorber.startLifeTime = Time.time + combinationSettings.bonusLifetime;

            if (combinationSettings.instantGain)
            {
                GiveNumber();
            }
        }
        public void GiveNumber()
        {
            if (!givenNumber)
            {
                givenNumber = true;

                myAbsorber.number += number;
                myAbsorber.UpdateText();
                myAbsorber.currentScale = new Vector3(myAbsorber.baseScale.x * combinationSettings.absorberScaleGain.x, myAbsorber.baseScale.y * combinationSettings.absorberScaleGain.y, 1);
            }
        }

        Vector3 ApplyShake(Vector3 vector, ShakeSettings shakeSettings)
        {
            float currentTime = Time.time - startTime;

            vector += transform.up * Mathf.Sin(shakeSettings.frequency * currentTime) * shakeSettings.offset.y;
            vector += transform.right * Mathf.Sin(shakeSettings.frequency * currentTime) * shakeSettings.offset.x;

            return vector;
        }

        public void GetReferences()
        {
            textA = transform.Find("TextA").GetComponent<TextMeshPro>();
            textB = transform.Find("TextB").GetComponent<TextMeshPro>();

            baseAlpha = 0.9f;
        }
        public void InitVariables()
        {
            currentFollowSpeed = 1f;
            startLifeTime = startTime = Time.time;
            position = transform.position;
            currentScale = baseScale = transform.localScale;

            //Movement:
            if (enableMovement)
            {
                if (moveType == MoveType.LERP)
                {
                    remainingOffset = new Vector2(Random.Range(lerpSettings.minX, lerpSettings.maxX), Random.Range(lerpSettings.minY, lerpSettings.maxY));
                }
                else
                {
                    currentVelocity = new Vector2(Random.Range(velocitySettings.minX, velocitySettings.maxX), Random.Range(velocitySettings.minY, velocitySettings.maxY));
                }
            }

            //Start Rotation:
            if (enableStartRotation)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Random.Range(minRotation, maxRotation));
            }

            //Start Faded:
            if (enableFading)
            {
                currentFade = 0f;
                SetFadeIn(0);
            }

            lastTargetPosition = Vector3.zero;

            //Update Text:
            UpdateText();
        }
        public Material GetMaterial()
        {
            return textA.fontSharedMaterial;
        }
        public TextMeshPro GetTextA()
        {
            return textA;
        }
        public TextMeshPro GetTextB()
        {
            return textB;
        }

        #region Text
        public void UpdateText()
        {
            string numberText = "";
            if(enableNumber)
            {
                string numberString = default;
                bool shortened;

                if (digitSettings.decimals <= 0)
                {
                    numberString = ProcessIntegers(Mathf.RoundToInt(number).ToString(), out shortened);
                }
                else
                {
                    string allDigits = Mathf.RoundToInt(number * Mathf.Pow(10, digitSettings.decimals)).ToString();
                    int usedDecimals = digitSettings.decimals;

                    while (digitSettings.hideZeros && allDigits.EndsWith("0") && usedDecimals > 0)
                    {
                        allDigits = allDigits.Substring(0, allDigits.Length - 1);
                        usedDecimals--;
                    }

                    string integers = allDigits.Substring(0, Mathf.Max(0, allDigits.Length - usedDecimals));

                    integers = ProcessIntegers(integers, out shortened);

                    if(integers == "")
                    {
                        integers = "0";
                    }

                    string decimals = allDigits.Substring(allDigits.Length - usedDecimals);

                    if (usedDecimals > 0 && !shortened)
                    {
                        numberString = integers + digitSettings.decimalChar + decimals;
                    }
                    else
                    {
                        numberString = integers;
                    }
                }

                numberText = ApplyTextSettings(numberString, numberSettings);
            }

            string prefixText = "";
            if (enablePrefix)
            {
                prefixText = ApplyTextSettings(prefix, prefixSettings);
            }

            string suffixText = "";
            if (enableSuffix)
            {
                suffixText = ApplyTextSettings(suffix, suffixSettings);
            }

            if (textA == null) GetReferences();
            textA.text = textB.text = prefixText + numberText + suffixText;
        }
        string ProcessIntegers(string integers, out bool shortened)
        {
            shortened = false;

            //Short Suffix:
            if (digitSettings.suffixShorten)
            {
                int currentSuffix = -1;

                while(integers.Length > digitSettings.maxDigits && currentSuffix < digitSettings.suffixes.Count - 1 && integers.Length - digitSettings.suffixDigits[currentSuffix + 1] > 0)
                {
                    currentSuffix++;
                    integers = integers.Substring(0, integers.Length - digitSettings.suffixDigits[currentSuffix]);
                }

                if(currentSuffix >= 0)
                {
                    integers += digitSettings.suffixes[currentSuffix];
                    shortened = true;
                    return integers;
                }
            }

            //Dots:
            if (digitSettings.dotSeperation && digitSettings.dotDistance > 0)
            {
                char[] chars = integers.ToCharArray();
                integers = "";
                for (int n = chars.Length - 1; n > -1; n--)
                {
                    integers = chars[n] + integers;

                    if ((chars.Length - n) % digitSettings.dotDistance == 0 && n > 0)
                    {
                        integers = digitSettings.dotChar + integers;
                    }
                }
            }

            return integers;
        }
        string ApplyTextSettings(string text, TextSettings settings)
        {
            string newString = text;

            if(text == "")
            {
                return "";
            }

            //Formatting:
            if (settings.bold)
            {
                newString = "<b>" + newString + "</b>";
            }
            if (settings.italic)
            {
                newString = "<i>" + newString + "</i>";
            }
            if (settings.underline)
            {
                newString = "<u>" + newString + "</u>";
            }
            if (settings.strike)
            {
                newString = "<s>" + newString + "</s>";
            }

            //Custom Color:
            if (settings.customColor)
            {
                newString = "<color=#" + ColorUtility.ToHtmlStringRGBA(settings.color) + ">" + newString + "</color>";
            }

            if (settings.mark)
            {
                newString = "<mark=#" + ColorUtility.ToHtmlStringRGBA(settings.markColor) + ">" + newString + "</mark>";
            }

            if (settings.alpha < 1)
            {
                newString = "<alpha=#" + ColorUtility.ToHtmlStringRGBA(new Color(1, 1, 1, settings.alpha)).Substring(6) + ">" + newString + "<alpha=#FF>";
            }

            //Change Size:
            if (settings.size > 0)
            {
                newString = "<size=+" + settings.size.ToString().Replace(',', '.') + ">" + newString + "</size>";
            }
            else if (settings.size < 0)
            {
                newString = "<size=-" + Mathf.Abs(settings.size).ToString().Replace(',', '.') + ">" + newString + "</size>";
            }

            //Character Spacing:
            if (settings.characterSpacing != 0)
            {
                newString = "<cspace=" + settings.characterSpacing.ToString().Replace(',', '.') + ">" + newString + "</cspace>";
            }

            //Spacing:
            if (settings.horizontal > 0)
            {
                string space = "<space=" + settings.horizontal.ToString().Replace(',', '.') + "em>";
                newString = space + newString + space;
            }

            if(settings.vertical != 0)
            {
                newString = "<voffset=" + settings.vertical.ToString().Replace(',', '.') + "em>" + newString + "</voffset>";
            }

            //Return:
            return newString;
        }
        #endregion

        public bool IsAlive()
        {
            return Time.time - startLifeTime < lifetime;
        }

        #region Fading
        void HandleFading()
        {
            if (enableFading == false) return; //Return if Fading is not enabled.

            if (IsAlive())
            {
                //Fading In:
                if(currentFade < 1)
                {
                    float fadeSpeed = 1f / Mathf.Max(0.0001f, fadeIn.fadeDuration);

                    currentFade = Mathf.Min(1, currentFade + Time.deltaTime * fadeSpeed);
                    SetFadeIn(currentFade);
                }
            }
            else
            {
                //Fading Out:
                if (currentFade > 0)
                {
                    float fadeSpeed = 1f / Mathf.Max(0.0001f, fadeOut.fadeDuration);

                    currentFade = Mathf.Min(1, currentFade - Time.deltaTime * fadeSpeed);
                    SetFadeOut(currentFade);
                    RemoveFromCombination();

                    if (currentFade <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
        public void SetFadeIn(float progress)
        {
            SetFade(progress, fadeIn);
        }
        public void SetFadeOut(float progress)
        {
            SetFade(progress, fadeOut);
        }
        public void SetFade(float progress, FadeSettings fadeSettings)
        {
            if (this == null) return;

            //Position Offset:
            textA.transform.localPosition = Vector2.Lerp(fadeSettings.positionOffset, Vector2.zero, progress);
            textB.transform.localPosition = -textA.transform.localPosition;

            //Scale & Scale Offset:
            Vector2 scaleOffset = fadeSettings.scaleOffset;
            if (scaleOffset.x == 0) scaleOffset.x += 0.001f;
            if (scaleOffset.y == 0) scaleOffset.y += 0.001f;

            Vector3 scaleA = Vector2.Lerp(scaleOffset * fadeSettings.scale, Vector2.one, progress);
            scaleA.z = 1;
            Vector3 scaleB = Vector2.Lerp(new Vector3(1f / scaleOffset.x, 1f / scaleOffset.y, 1) * fadeSettings.scale, Vector2.one, progress);
            scaleB.z = 1;

            textA.transform.localScale = scaleA;
            textB.transform.localScale = scaleB;

            //Alpha:
            UpdateAlpha(progress);
        }
        public void UpdateAlpha(float progress)
        {
            textA.alpha = textB.alpha = Mathf.Clamp01(progress * progress * baseAlpha * baseAlpha);
        }
        #endregion

        private void OnDestroy()
        {
            RemoveFromCombination();
        }

        public void RemoveFromCombination()
        {
            if (enableCombination && !removedFromDictionary && combinationSettings.combinationGroup != "")
            {
                if (combinationDictionary != null && combinationDictionary.ContainsKey(combinationSettings.combinationGroup))
                {
                    if (combinationDictionary[combinationSettings.combinationGroup].Contains(this))
                    {
                        removedFromDictionary = true;
                        combinationDictionary[combinationSettings.combinationGroup].Remove(this);
                    }
                }
            }
        }
    }
}
