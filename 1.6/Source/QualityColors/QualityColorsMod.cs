using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace QualityColors;

public class QualityColorsMod : Mod
{
	private static readonly Regex ColorMatcher = new Regex("<color=#([0-9A-F]{3,6})>(.*?)</color>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft);

	private static readonly Dictionary<Thing, QualityCategory> qualCache = new Dictionary<Thing, QualityCategory>();

	private static readonly HashSet<ThingDef> qualityLess = new HashSet<ThingDef>();

	public static Harmony Harm;

	public static ColorSettings Settings;

	public QualityColorsMod(ModContentPack content)
		: base(content)
	{
		Harm = new Harmony("legodude17.QualityColors");
		Settings = GetSettings<ColorSettings>();
		Harm.Patch(AccessTools.Method(typeof(TransferableUIUtility), "DrawTransferableInfo"), new HarmonyMethod(GetType(), "AddColors"));
		Harm.Patch(AccessTools.Method(typeof(CompQuality), "CompInspectStringExtra"), null, new HarmonyMethod(typeof(QualityColorsMod), "AddColor3"));
		Harm.Patch(AccessTools.Method(typeof(QualityUtility), "GetLabel"), null, new HarmonyMethod(typeof(QualityColorsMod), "AddColor2"));
		Harm.Patch(AccessTools.Method(typeof(QualityUtility), "GetLabelShort"), null, new HarmonyMethod(typeof(QualityColorsMod), "AddColor2"));
		Harm.Patch(AccessTools.Method(typeof(GenText), "Truncate", new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(Dictionary<string, string>)
		}), new HarmonyMethod(GetType(), "StripColor"), new HarmonyMethod(GetType(), "ReaddColor"));
		Harm.Patch(AccessTools.Method(typeof(GenText), "Truncate", new Type[3]
		{
			typeof(TaggedString),
			typeof(float),
			typeof(Dictionary<string, TaggedString>)
		}), new HarmonyMethod(GetType(), "StripColorTagged"), new HarmonyMethod(GetType(), "ReaddColorTagged"));
		ApplySettings();
	}

	public override string SettingsCategory()
	{
		return "QualityColors".Translate();
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		base.DoSettingsWindowContents(inRect);
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(inRect);
		listing_Standard.CheckboxLabeled("QualityColors.FullLabel.Label".Translate(), ref Settings.FullLabel, "QualityColors.FullLabel.Tooltip".Translate());
		listing_Standard.Label("QualityColors.Colors.Label".Translate());
		if (listing_Standard.ButtonText("QualityColors.Presets".Translate()))
		{
			Find.WindowStack.Add(new FloatMenu(ColorSettings.Presets.Keys.Select((string key) => new FloatMenuOption(("QualityColors.Presets." + key).Translate(), delegate
			{
				Settings.Colors = ColorSettings.Presets[key].ToDictionary((KeyValuePair<QualityCategory, Color> kv) => kv.Key, (KeyValuePair<QualityCategory, Color> kv) => kv.Value);
			})).ToList()));
		}
		foreach (QualityCategory cat in QualityUtility.AllQualityCategories)
		{
			if (Widgets.ButtonText(listing_Standard.GetRect(40f), "QualityColors.Change".Translate(cat.GetLabel()), drawBackground: false, doMouseoverSound: true, Settings.Colors[cat]))
			{
				FakeGlower glower = new FakeGlower(ColorInt.FromHdrColor(Settings.Colors[cat]), delegate(ColorInt color)
				{
					Settings.Colors[cat] = color.ToColor;
				});
				CustomDialog_GlowerPicker window = new CustomDialog_GlowerPicker(glower, new List<CompGlower>(), Widgets.ColorComponents.All, Widgets.ColorComponents.All);
				Find.WindowStack.Add(window);
			}
		}
		listing_Standard.End();
	}

	public static void StripColor(ref string str, out Dictionary<string, string> __state, Dictionary<string, string> cache = null)
	{
		__state = new Dictionary<string, string>();
		if (str.NullOrEmpty())
		{
			return;
		}
		string text = ColorMatcher.Replace(str, (Match match2) => match2.Groups[2].Value, int.MaxValue);
		if (cache != null && cache.ContainsKey(text))
		{
			str = text;
			return;
		}
		foreach (Match item in ColorMatcher.Matches(str))
		{
			__state.Add(item.Groups[2].Value, item.Groups[1].Value);
		}
		str = text;
	}

	public static void ReaddColor(ref string __result, string str, Dictionary<string, string> __state, Dictionary<string, string> cache = null)
	{
		foreach (string key in __state.Keys)
		{
			string text = __result.Replace(key, "<color=#" + __state[key] + ">" + key + "</color>");
			if (text == __result)
			{
				for (int num = key.Length - 1; num >= 1; num--)
				{
					text = __result.Replace(key.Substring(0, num) + "...", "<color=#" + __state[key] + ">" + key.Substring(0, num) + "</color>...");
					if (text != __result)
					{
						break;
					}
				}
			}
			__result = text;
		}
		if (cache != null)
		{
			cache[str] = __result;
		}
	}

	public static void StripColorTagged(ref TaggedString str, out Dictionary<string, string> __state, Dictionary<string, TaggedString> cache = null)
	{
		string str2 = str.RawText;
		Dictionary<string, string> cache2 = cache?.Select((KeyValuePair<string, TaggedString> kv) => (Key: kv.Key, RawText: kv.Value.RawText)).ToDictionary(((string Key, string RawText) val) => val.Key, ((string Key, string RawText) val) => val.RawText);
		StripColor(ref str2, out __state, cache2);
		str = str2;
	}

	public static void ReaddColorTagged(ref TaggedString __result, TaggedString str, Dictionary<string, string> __state, Dictionary<string, TaggedString> cache = null)
	{
		string __result2 = __result.RawText;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		ReaddColor(ref __result2, str.RawText, __state, dictionary);
		if (cache != null)
		{
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				cache[item.Key] = item.Value;
			}
		}
		__result = __result2;
	}

	private static bool TryGetQuality(Thing t, out QualityCategory cat)
	{
		if (t == null || qualityLess.Contains(t.def))
		{
			cat = QualityCategory.Normal;
			return false;
		}
		if (qualCache.TryGetValue(t, out cat))
		{
			return true;
		}
		if (t.TryGetQuality(out cat))
		{
			qualCache.Add(t, cat);
			return true;
		}
		qualityLess.Add(t.def);
		return false;
	}

	public static void AddColor(MainTabWindow_Inspect __instance)
	{
		if (Find.Selector.SingleSelectedThing != null && (Find.Selector.NumSelected == 1 || Find.Selector.SelectedObjects.OfType<Thing>().Select(delegate(Thing t)
		{
			TryGetQuality(t, out var cat2);
			return cat2;
		}).Distinct()
			.Count() == 1) && TryGetQuality(Find.Selector.SingleSelectedThing, out var cat))
		{
			GUI.color = Settings.Colors[cat];
		}
	}

	public static void AddColor2(QualityCategory cat, ref string __result)
	{
		__result = ColorText(__result, Settings.Colors[cat]);
	}

	public static void AddColor3(CompQuality __instance, ref string __result)
	{
		__result = ColorText(__result, Settings.Colors[__instance.Quality]);
	}

	public static string ColorText(string text, Color color)
	{
		return $"<color=#{Mathf.RoundToInt(color.r * 255f):X2}{Mathf.RoundToInt(color.g * 255f):X2}{Mathf.RoundToInt(color.b * 255f):X2}>{text}</color>";
	}

	public static void AddColors(Transferable trad, Rect idRect, ref Color labelColor)
	{
		if (labelColor == Color.white && trad.IsThing && TryGetQuality(trad.AnyThing, out var cat))
		{
			labelColor = Settings.Colors[cat];
		}
	}

	public override void WriteSettings()
	{
		base.WriteSettings();
		ApplySettings();
	}

	public void ApplySettings()
	{
		(AccessTools.Field(typeof(GenLabel), "labelDictionary").GetValue(null) as IDictionary)?.Clear();
		(AccessTools.Field(typeof(InspectPaneUtility), "truncatedLabelsCached").GetValue(null) as IDictionary)?.Clear();
		Harm.Unpatch(AccessTools.Method(typeof(MainTabWindow_Inspect), "GetLabel"), HarmonyPatchType.Postfix, Harm.Id);
		if (Settings.FullLabel)
		{
			Harm.Patch(AccessTools.Method(typeof(MainTabWindow_Inspect), "GetLabel"), null, new HarmonyMethod(typeof(QualityColorsMod), "AddColor"));
		}
	}
}
