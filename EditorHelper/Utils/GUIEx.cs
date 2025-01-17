﻿using System;
using System.Globalization;
using System.Linq;
using EditorHelper.Settings;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Utils {
    public static class GUIEx {
        public static bool DisableAll;
        public static void BeginIndent(int indent = 30) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            GUILayout.BeginVertical();
        }
        
        public static void EndIndent() {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        
        public static void Label(params (LangCode, string)[]? labels) {
            GUILayout.Label(CheckLangCode(labels));
        }
        
        public static void Label(int width, Action? gui, params (LangCode, string)[]? labels) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CheckLangCode(labels), GUILayout.Width(width));
            gui?.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        public static void Label(int width, Action? gui, int size, params (LangCode, string)[]? labels) {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<size={size}>{CheckLangCode(labels)}</size>", GUILayout.Width(width));
            gui?.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        public static void Toggle(ref bool value, params (LangCode, string)[]? labels) {
            if (DisableAll) GUILayout.Toggle(value, labels == null ? "" : " " + CheckLangCode(labels));
            else value = GUILayout.Toggle(value, labels == null ? "" : " " + CheckLangCode(labels));
            GUILayout.Space(5);
        }
        public static void IntField(ref int value, params (LangCode, string)[]? labels) {
            var toCheck = value.ToString();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            toCheck = GUILayout.TextField(toCheck, GUILayout.Width(25));
            if (labels != null) GUILayout.Label(" " + CheckLangCode(labels));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (toCheck == "-") toCheck = "0";
            if (toCheck == "") toCheck = "0";
            if (!DisableAll && int.TryParse(toCheck, out int val)) {
                value = val;
            }
        }
        
        public static void IntField(ref int value, int min = int.MinValue, int max = int.MaxValue, int width = 30) {
            var toCheck = value.ToString();
            toCheck = GUILayout.TextField(toCheck, GUILayout.Width(width));
            if (toCheck == "-") toCheck = "0";
            if (toCheck == "") toCheck = "0";
            if (!DisableAll && int.TryParse(toCheck, out int val)) {
                if (val < min || val > max) return;
                value = val;
            }
        }
        
        public static void FloatField(ref float value, float min = float.NegativeInfinity, float max = float.PositiveInfinity, int width = 30) {
            var toCheck = value.ToString(CultureInfo.InvariantCulture);
            toCheck = GUILayout.TextField(toCheck, GUILayout.Width(width));
            if (toCheck == "-") toCheck = "0";
            if (toCheck == "") toCheck = "0";
            if (toCheck.EndsWith(".")) toCheck = toCheck + "0";
            if (!DisableAll && int.TryParse(toCheck, out int val)) {
                if (val < min || val > max) return;
                value = val;
            }
        }
        public static void FractionField(ref Fraction value, Fraction? min = null, Fraction? max = null) {
            min ??= Fraction.MinValue;
            max ??= Fraction.MaxValue;
            GUILayout.BeginHorizontal();
            var tNumerator = value.Numerator + value.Denominator * value.Integer;
            var denominator = (int) value.Denominator;
            IntField(ref tNumerator, 0);
            GUILayout.Label("/", GUILayout.Width(15));
            IntField(ref denominator, 1);
            Fraction result;
            while (true) {
                result = new Fraction(tNumerator, (uint) denominator);
                if (result < min) {
                    tNumerator++;
                } else if (result > max) {
                    tNumerator -= 1;
                } else break;

                if (result > max) break;
            }
            GUILayout.Label($"({((double) result).ToString()})", GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            if (result < min || result > max) return;
            value = result;
        }

        public static void TextField(ref string value, int width = 100) {
            if (DisableAll) GUILayout.TextField(value, GUILayout.Width(width));
            else value = GUILayout.TextField(value, GUILayout.Width(width));
        }

        private static KeyMap? _currentKeymap;
        public static void KeyMap(ref KeyMap keyMap, params (LangCode, string)[]? labels) {
            GUILayout.Label($"<size=15>{CheckLangCode(labels)}</size>");
            GUILayout.Space(-5);
            GUILayout.BeginHorizontal();
            if (DisableAll) {
                GUILayout.Toggle(keyMap.NeedsCtrl, " <size=13>Ctrl</size>", GUILayout.Width(60));
                GUILayout.Toggle(keyMap.NeedsShift, " <size=13>Shift</size>", GUILayout.Width(60));
                GUILayout.Toggle(keyMap.NeedsAlt, " <size=13>Alt</size>", GUILayout.Width(60));
                GUILayout.Toggle(keyMap.NeedsBackQuote, " <size=16>~</size>", GUILayout.Width(60));
            } else {
                keyMap.NeedsCtrl = GUILayout.Toggle(keyMap.NeedsCtrl, " <size=13>Ctrl</size>", GUILayout.Width(60));
                keyMap.NeedsShift = GUILayout.Toggle(keyMap.NeedsShift, " <size=13>Shift</size>", GUILayout.Width(60));
                keyMap.NeedsAlt = GUILayout.Toggle(keyMap.NeedsAlt, " <size=13>Alt</size>", GUILayout.Width(60));
                keyMap.NeedsBackQuote = GUILayout.Toggle(keyMap.NeedsBackQuote, " <size=16>~</size>", GUILayout.Width(60));
            }

            var keycode = keyMap.GetKeycodeValue();
            if (keyMap.KeyCode != KeyCode.None) {
                GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
                GUI.skin.textField.richText = true;
                GUILayout.TextField(keycode, GUI.skin.textField, GUILayout.Width(Math.Max(keycode.RemoveRichTags().Length * 12, 30)), GUILayout.Height(30));
                GUI.skin.textField.alignment = TextAnchor.UpperLeft;
                GUI.skin.textField.richText = false;
                
                GUILayout.Space(10);
                if (!DisableAll && _currentKeymap == keyMap) {
                    if (Event.current.isKey && Event.current.type == EventType.KeyDown) {
                        var pressed = Event.current.keyCode;
                        if (pressed != KeyCode.None) {
                            keyMap.KeyCode = pressed;
                            if (pressed == KeyCode.Escape) 
                                _currentKeymap = null;
                        }
                    }

                    if (!DisableAll && GUILayout.Button("End Edit", GUILayout.Width(80))) {
                        _currentKeymap = null;
                    }
                } else {
                    if (!DisableAll && GUILayout.Button("Edit", GUILayout.Width(80))) {
                        _currentKeymap = keyMap;
                    }
                }
                
            }

            GUILayout.Space(10);
            var reset = GUILayout.Button(keyMap == keyMap.Inital ? "" : "Reset", keyMap == keyMap.Inital ? GUIStyle.none : GUI.skin.button, GUILayout.Width(60), GUILayout.Height(16));
            if (!DisableAll && reset && keyMap != keyMap.Inital) {
                keyMap.Reset();
                _currentKeymap = null;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        public static string CheckLangCode(params (LangCode, string)[]? codes) {
            if (codes == null) return "TRANSLATION_FAILED";
            var found = codes.Where(c => c.Item1 == Localization.CurrentLanguage).ToArray();
            if (found.Any()) {
                return found[0].Item2;
            }
            found = codes.Where(c => c.Item1 == LangCode.English).ToArray();
            if (found.Any()) {
                return found[0].Item2;
            }

            return "TRANSLATION_FAILED";
        }
    }
}