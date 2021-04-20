using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.Rendering;

namespace DamageNumbersPro
{
    [CustomEditor(typeof(DamageNumber)), CanEditMultipleObjects]
    public class DamageNumberEditor : Editor
    {
        //References:
        DamageNumber dn;
        TextMeshPro textA;
        TextMeshPro textB;
        Material mat;

        //Styles:
        GUIStyle style;

        //Preview:
        float currentFadeIn;
        float currentFadeOut;

        //Tips:
        bool spawnHelp;
        bool glowHelp;
        bool overlayHelp;

        //Help
        bool numberHelp;
        bool prefixHelp;
        bool suffixHelp;
        bool fadingHelp;
        bool movementHelp;
        bool shakingHelp;
        bool startRotationHelp;
        bool followingHelp;
        bool combinationHelp;
        bool perspectiveHelp;


        //Repaint:
        public bool repaintViews;

        //Current:
        public static int currentEditor;

        //External Inspectors:
        MaterialEditor matEditor;
        Editor textEditor;

        void OnEnable()
        {
            style = new GUIStyle();
            style.richText = true;
            
            dn = (DamageNumber)target;
            try
            {
                dn.GetReferences();
            }catch
            {

                return;
            }

            if (!Application.isPlaying)
            {
                currentFadeIn = currentFadeOut = 0;
                dn.SetFadeIn(1);
            }

            textA = dn.GetTextA();
            textB = dn.GetTextB();
        }

        public static void Prepare(GameObject go)
        {
            if(go.GetComponent<SortingGroup>() == null)
            {
                go.AddComponent<SortingGroup>().sortingOrder = 1000;
            }
            
            if(go.transform.Find("TextA") == null)
            {
                NewTextMesh("TextA", go.transform);
            }

            if (go.transform.Find("TextB") == null)
            {
                NewTextMesh("TextB", go.transform);
            }

            Undo.RegisterCreatedObjectUndo(go, "Create new Damage Number");
        }
        public static GameObject NewTextMesh(string tmName, Transform parent)
        {
            GameObject newTM = new GameObject();
            newTM.name = tmName;

            TextMeshPro tmp = newTM.AddComponent<TextMeshPro>();
            tmp.fontSize = 5;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.text = "1";

            newTM.transform.SetParent(parent,true);
            newTM.transform.localPosition = Vector3.zero;
            newTM.transform.localScale = Vector3.one;
            newTM.transform.localEulerAngles = Vector3.zero;

            return newTM;
        }

        [MenuItem("GameObject/2D Object/Damage Number",priority = -1)]
        public static void CreateDamageNumber()
        {
            GameObject newDN = new GameObject("Damage Number");
            newDN.AddComponent<DamageNumber>();
            Prepare(newDN);

            if(Camera.main != null)
            {
                newDN.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
            }
        }

        public override void OnInspectorGUI()
        {
            if (repaintViews)
            {
                repaintViews = false;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }

            //Top:
            EditorGUILayout.BeginVertical("Helpbox");
            EditorGUILayout.LabelField("<size=15><b>Damage Numbers Pro:</b></size>", style);
            EditorGUILayout.LabelField("Highly customizable number or text pop ups.", style);
            EditorGUILayout.LabelField("Most properties have a <b>tooltip</b> if you hover over them.", style);

            Lines(); //Spawn Help:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("How can I spawn damage numbers ?");
            HelpToggle(ref spawnHelp);
            EditorGUILayout.EndHorizontal();
            if (spawnHelp)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<b>Like any other gameobject.</b>", style);
                EditorGUILayout.LabelField("Create and save your damage numbers as <b>prefabs</b>.", style);
                EditorGUILayout.LabelField("Spawn prefabs using <b>Instantiate<GameObject>(prefab)</b>", style);
                EditorGUILayout.LabelField("Or use <b>DamageNumber.CreateNew(number,position)</b> on the prefab.", style);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("You can see an example in the documentation and asset store video.", style);
            }
            Lines(); //Glow Help:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("How can I get a glowy effect ?");
            HelpToggle(ref glowHelp);
            EditorGUILayout.EndHorizontal();
            if (glowHelp)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Enable <b>HDR</b> and use <b>bloom</b> post processing.", style);
                EditorGUILayout.LabelField("The process may differ based on the render pipeline.", style);
                EditorGUILayout.LabelField("Go to the <b>text material</b> and increase the face color intensity.", style);
            }
            Lines(); //3D Overlay Help:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("How can I get numbers to render above all 3D models ?");
            HelpToggle(ref overlayHelp);
            EditorGUILayout.EndHorizontal();
            if (overlayHelp)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Go to the <b>text material</b> and change the shader.", style);
                EditorGUILayout.LabelField("Use <b>Distance Field Overlay</b> (a textmeshpro shader).", style);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (textA == null || textB == null)
            {
                if (GUILayout.Button("Prepare"))
                {
                    Prepare(dn.gameObject);
                    OnEnable();
                }
                EditorGUILayout.LabelField("", style);
                EditorGUILayout.LabelField("Click the button above to prepare the gameobject.", style);
                EditorGUILayout.LabelField("Or use <b>[GameObject/2D Object/Damage Number]</b> to create a number.", style);
                return;
            }

            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            //Properties:
            DisplayMainSettings();
            DisplayNumber();
            DisplayPrefix();
            DisplaySuffix();
            DisplayFading();
            DisplayMovement();
            DisplayShaking();
            DisplayStartRotation();
            DisplayCombination();
            DisplayFollowing();
            DisplayPerspective();

            EditorGUILayout.EndVertical();

            //Fix Variables:
            FixTextSettings(ref dn.numberSettings);
            FixTextSettings(ref dn.prefixSettings);
            FixTextSettings(ref dn.suffixSettings);
            FixFadeSettings(ref dn.fadeIn);
            FixFadeSettings(ref dn.fadeOut);
            MinZero(ref dn.digitSettings.decimals);
            MinZero(ref dn.digitSettings.dotDistance);

            //Apply Properties:
            serializedObject.ApplyModifiedProperties();

            //Update Text:
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 1)
            {
                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    if (gameObject != dn.gameObject)
                    {
                        DamageNumber other = gameObject.GetComponent<DamageNumber>();
                        if (other != null)
                        {
                            other.UpdateText();
                        }
                    }
                }
            }
            dn.UpdateText();

            //Preview
            FadePreview();

            //External Editors:
            ExternalEditors();

            GUI.color = new Color(1, 1, 1, 0.7f);
            //End:
            EditorGUILayout.LabelField("If you have any problems, questions or feature-requests.",style);
            EditorGUILayout.LabelField("Send me an email: <b>ekincantascontact@gmail.com</b>",style);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Thanks for using my asset.", style);
            EditorGUILayout.LabelField("Reviews are appreciated :)", style);
            GUI.color = new Color(1, 1, 1, 1f);
        }

        #region Properties
        void DisplayMainSettings()
        {
            //Category:
            NewCategoryHorizontal("Main Settings");

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Main Settings");
                dn.lifetime = 2;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            if (dn.lifetime < 0) dn.lifetime = 0;

            //Properties:
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lifetime"));
        }
        void DisplayNumber()
        {
            //Category:
            NewCategoryHorizontal("Number");

            //Help:
            HelpToggle(ref numberHelp);

            //Toggle:
            EasyToggle(ref dn.enableNumber);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Number");
                dn.numberSettings = new TextSettings(0);
                dn.number = 1;
                dn.digitSettings = new DigitSettings(0);
                dn.enableNumber = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableNumber == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("number"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("numberSettings"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("digitSettings"));
            EditorGUILayout.EndHorizontal();

            //Help:
            if (numberHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- Displays a single <b>number</b>.", style);
                EditorGUILayout.LabelField("- Can be <b>disabled</b> if you only want text.", style);
                EditorGUILayout.LabelField("- Call <b>DamageNumber.UpdateText()</b> after runtime changes.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayPrefix()
        {
            //Category:
            NewCategoryHorizontal("Prefix");

            //Help:
            HelpToggle(ref prefixHelp);

            //Toggle:
            EasyToggle(ref dn.enablePrefix);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Prefix");
                dn.prefix = "";
                dn.prefixSettings = new TextSettings(0.2f);
                dn.enablePrefix = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enablePrefix == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prefix"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prefixSettings"));
            EditorGUILayout.EndHorizontal();

            //Help:
            if (prefixHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- Displays text to the <b>left</b> of the number.", style);
                EditorGUILayout.LabelField("- Call <b>DamageNumber.UpdateText()</b> after runtime changes.", style);

                GUI.color = Color.white;
            }
        }
        void DisplaySuffix()
        {
            //Category:
            NewCategoryHorizontal("Suffix");

            //Help:
            HelpToggle(ref suffixHelp);

            //Toggle:
            EasyToggle(ref dn.enableSuffix);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Suffix");
                dn.suffix = "";
                dn.suffixSettings = new TextSettings(0.2f);
                dn.enableSuffix = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableSuffix == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("suffix"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("suffixSettings"));
            EditorGUILayout.EndHorizontal();

            //Help:
            if (suffixHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- Displays text to the <b>right</b> of the number.", style);
                EditorGUILayout.LabelField("- Call <b>DamageNumber.UpdateText()</b> after runtime changes.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayFading()
        {
            //Category:
            NewCategoryHorizontal("Fading");

            //Help:
            HelpToggle(ref fadingHelp);

            //Toggle:
            EasyToggle(ref dn.enableFading);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Fading");
                dn.fadeIn = new FadeSettings(new Vector2(2, 2));
                dn.fadeOut = new FadeSettings(new Vector2(1, 1));
                dn.enableFading = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableFading == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeIn"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeOut"));
            EditorGUILayout.EndHorizontal();

            //Help:
            if (fadingHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- <b>Fades</b> the text in and out.", style);
                EditorGUILayout.LabelField("- <b>Postion Offset</b> moves the texts into opposite directions.", style);
                EditorGUILayout.LabelField("- <b>Scale Offset</b> scales up TextA and scales down TextB.", style);
                EditorGUILayout.LabelField("- <b>Scale</b> scales both texts.", style);
                EditorGUILayout.LabelField("- You can <b>preview</b> these using the sliders below.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayStartRotation()
        {
            //Category:
            NewCategoryHorizontal("Start Rotation");

            //Help:
            HelpToggle(ref startRotationHelp);

            //Toggle:
            EasyToggle(ref dn.enableStartRotation);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Start Rotation");
                dn.minRotation = -2;
                dn.maxRotation = 2;
                dn.enableStartRotation = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableStartRotation == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minRotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRotation"));

            //Help:
            if (startRotationHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- Spawns with a random <b>rotation</b> between <b>" + dn.minRotation + "</b> and <b>" + dn.maxRotation +"</b>.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayMovement()
        {
            //Category:
            NewCategoryHorizontal("Movement");

            //Help:
            HelpToggle(ref movementHelp);

            //Toggle:
            EasyToggle(ref dn.enableMovement);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Movement");
                dn.moveType = MoveType.LERP;
                dn.lerpSettings = new LerpSettings(0);
                dn.velocitySettings = new VelocitySettings(0);
                dn.enableMovement = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableMovement == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveType"));
            if(dn.moveType == MoveType.LERP)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("  ", GUILayout.Width(9));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lerpSettings"));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("  ", GUILayout.Width(9));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("velocitySettings"));
                EditorGUILayout.EndHorizontal();
            }

            //Help:
            if (movementHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                if(dn.moveType == MoveType.LERP)
                {
                    EditorGUILayout.LabelField("- <b>Lerps</b> towards a random offset.", style);
                    EditorGUILayout.LabelField("- <b>Speed</b> can be adjusted.", style);
                }
                else
                {
                    EditorGUILayout.LabelField("- Spawns with a random <b>velocity</b>.", style);
                    EditorGUILayout.LabelField("- Velocity can be influenced by <b>drag</b> and <b>gravity</b>.", style);
                }

                GUI.color = Color.white;
            }
        }
        void DisplayShaking()
        {
            //Category:
            NewCategoryHorizontal("Shaking");

            //Help:
            HelpToggle(ref shakingHelp);

            //Toggle:
            EasyToggle(ref dn.enableShaking);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Shaking");
                dn.idleShake = new ShakeSettings(new Vector2(0.005f, 0.005f));
                dn.fadeInShake = new ShakeSettings(new Vector2(0f, 0f));
                dn.fadeOutShake = new ShakeSettings(new Vector2(0f, 0f));
                dn.enableShaking = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableShaking == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleShake"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeInShake"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeOutShake"));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            //Help:
            if (shakingHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- <b>Shakes</b> back and forth between <b>-offset</b> and +<b>offset</b>.", style);
                EditorGUILayout.LabelField("- Uses <b>FadeInShake</b> and <b>FadeOutShake</b> while fading.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayCombination()
        {
            //Category:
            NewCategoryHorizontal("Combination");

            //Help:
            HelpToggle(ref combinationHelp);

            //Toggle:
            EasyToggle(ref dn.enableCombination);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Combination");
                dn.combinationSettings = new CombinationSettings(0f);
                dn.enableCombination = false;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableCombination == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("combinationSettings"));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            //Tip:
            if(dn.enableCombination && dn.combinationSettings.combinationGroup == "")
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- Please fill out the <b>Combination Group</b>.", style);
                EditorGUILayout.LabelField("- Otherwise numbers won't combine.", style);

                GUI.color = Color.white;
            }

            //Help:
            if (combinationHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- <b>Combines</b> numbers of the same <b>Combination Group</b>.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayFollowing()
        {
            //Category:
            NewCategoryHorizontal("Following");

            //Help:
            HelpToggle(ref followingHelp);

            //Toggle:
            EasyToggle(ref dn.enableFollowing);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Following");
                dn.followedTarget = null;
                dn.followSpeed = 10;
                dn.followDrag = 0;
                dn.enableFollowing = true;
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enableFollowing == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("followedTarget"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("followSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("followDrag"));
            GUI.enabled = true;

            //Help:
            if (followingHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- <b>Follows</b> the <b>target</b> around.", style);
                EditorGUILayout.LabelField("- Will try to maintain it's position <b>relative</b> to the target.", style);
                EditorGUILayout.LabelField("- <b>Drag</b> fades out the following.", style);
                EditorGUILayout.LabelField("- Can be used to make damage numbers <b>track</b> their enemy.", style);

                GUI.color = Color.white;
            }
        }
        void DisplayPerspective()
        {
            //Category:
            NewCategoryHorizontal("Perspective Camera");

            //Help:
            HelpToggle(ref perspectiveHelp);

            //Toggle:
            EasyToggle(ref dn.enablePerspective);

            //Reset:
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                Undo.RecordObject(target, "Reset Perspective");
                dn.cameraOverride = null;
                dn.consistentScale = true;
                dn.enablePerspective = true;
                dn.perspectiveSettings = new PerspectiveSettings(0);
                RefreshText();
            }
            EditorGUILayout.EndHorizontal();

            //Properties:
            if (dn.enablePerspective == false)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraOverride"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("consistentScale"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  ", GUILayout.Width(9));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("perspectiveSettings"));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            //Help:
            if (perspectiveHelp)
            {
                Lines();
                GUI.color = new Color(1, 1, 1, 0.7f);

                EditorGUILayout.LabelField("- <b>Looks at</b> the <b>Main Camera</b> or <b>Override Camera</b>.", style);
                EditorGUILayout.LabelField("- Keeps the size consistent across different camera distances.", style);
                EditorGUILayout.LabelField("- Ignore this if you are creating a 2D game or use a orthographic camera.", style);

                GUI.color = Color.white;
            }
        }
        #endregion

        void HelpToggle(ref bool helpVariable)
        {
            helpVariable = GUILayout.Toggle(helpVariable,"?", GUI.skin.button, GUILayout.Width(20));
        }

        void EasyToggle(ref bool toggleVariable)
        {
            bool oldToggle = toggleVariable;
            bool newToggle = GUILayout.Toggle(toggleVariable, toggleVariable ? "Enabled" : "Disabled", GUI.skin.button, GUILayout.Width(60));

            if(oldToggle != newToggle)
            {
                Undo.RecordObject(target, newToggle ? "Enable Setting" : "Disable Setting");
                toggleVariable = newToggle;
                RefreshText();
            }
        }

        void RefreshText()
        {
            repaintViews = true;
            GUI.FocusControl("");
        }

        void FixTextSettings(ref TextSettings ts)
        {
            if (ts.horizontal < 0)
            {
                ts.horizontal = 0;
            }
        }
        void FixFadeSettings(ref FadeSettings fs)
        {
            if (fs.fadeDuration < 0)
            {
                fs.fadeDuration = 0;
            }
        }
        void MinZero(ref int variable)
        {
            if(variable < 0)
            {
                variable = 0;
            }
        }

        void NewCategory(string title)
        {
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Helpbox");
            EditorGUILayout.LabelField("<b>"+ title + ":</b>", style);
        }
        void NewCategoryHorizontal(string title)
        {
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Helpbox");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<b>" + title + ":</b>", style);
        }

        void ExternalEditors()
        {
            int oldEditor = currentEditor;
            EditorGUILayout.LabelField("<b>Other Inspectors:</b>", style);
            currentEditor = GUILayout.Toolbar(currentEditor, new string[] { "None", "Material", "TextMeshPro" });
            Lines();
            if (currentEditor == 1)
            {
                if(matEditor == null || oldEditor != 1)
                {
                    mat = dn.GetMaterial();
                    if (matEditor != null)
                    {
                        DestroyImmediate(matEditor);
                    }
                    matEditor = (MaterialEditor)CreateEditor(mat);
                }

                if (matEditor != null)
                {
                    matEditor.DrawHeader();
                    matEditor.OnInspectorGUI();
                    Lines();
                }
            }
            else if (currentEditor == 2)
            {
                if (textEditor == null || oldEditor != 2)
                {
                    if (textEditor != null)
                    {
                        DestroyImmediate(textEditor);
                    }
                    textEditor = CreateEditor(textA);
                }

                if (textEditor != null)
                {
                    textEditor.DrawHeader();
                    textEditor.OnInspectorGUI();
                    EditorUtility.CopySerializedIfDifferent(textA, textB);


                    if(currentFadeIn == 0 && currentFadeOut == 0)
                    {
                        dn.UpdateAlpha(1);
                    }

                    textB.GetComponent<MeshRenderer>().material = textA.GetComponent<MeshRenderer>().material = textA.font.material;

                    Lines();
                }
            }
        }

        void Lines()
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            EditorGUILayout.LabelField("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
            GUI.color = new Color(1, 1, 1, 1f);
        }

        void FadePreview()
        {
            EditorGUILayout.Space();
            Lines();
            EditorGUILayout.LabelField("<b>Preview:</b>", style);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Fade In");
            float lastFadeIn = currentFadeIn;
            currentFadeIn = GUILayout.HorizontalSlider(currentFadeIn, 0, 1);
            GUILayout.Label(Mathf.RoundToInt(currentFadeIn * 100f) + "%", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            if (currentFadeIn != lastFadeIn || currentFadeIn > 0)
            {
                currentFadeOut = 0;
                dn.SetFadeIn(currentFadeIn);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Fade Out");
            float lastFadeOut = currentFadeOut;
            currentFadeOut = GUILayout.HorizontalSlider(currentFadeOut, 0, 1);
            GUILayout.Label(Mathf.RoundToInt(currentFadeOut * 100f) + "%", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            if (currentFadeOut != lastFadeOut || currentFadeOut > 0)
            {
                currentFadeIn = 0;
                dn.SetFadeOut(1 - currentFadeOut);
            }
            Lines();
        }

        void OnDisable()
        {
            if (!Application.isPlaying && textA != null)
            {
                currentFadeIn = currentFadeOut = 0;
                dn.SetFadeIn(1);
            }

            if (matEditor != null)
            {
                DestroyImmediate(matEditor);
            }
            if (textEditor != null)
            {
                DestroyImmediate(textEditor);
            }
        }

    }
}
