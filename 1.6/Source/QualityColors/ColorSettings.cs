using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace QualityColors;

public class ColorSettings : ModSettings
{
	public static Dictionary<string, Dictionary<QualityCategory, Color>> Presets = new Dictionary<string, Dictionary<QualityCategory, Color>>
	{
		{
			"Legacy",
			new Dictionary<QualityCategory, Color>
			{
				{
					QualityCategory.Awful,
					Color.red
				},
				{
					QualityCategory.Poor,
					Color.red
				},
				{
					QualityCategory.Normal,
					Color.white
				},
				{
					QualityCategory.Good,
					Color.cyan
				},
				{
					QualityCategory.Excellent,
					Color.green
				},
				{
					QualityCategory.Masterwork,
					Color.green
				},
				{
					QualityCategory.Legendary,
					Color.yellow
				}
			}
		},
		{
			"WoW",
			new Dictionary<QualityCategory, Color>
			{
				{
					QualityCategory.Awful,
					new Color(0.41f, 0.41f, 0.41f)
				},
				{
					QualityCategory.Poor,
					new Color(0.62f, 0.62f, 0.62f)
				},
				{
					QualityCategory.Normal,
					Color.white
				},
				{
					QualityCategory.Good,
					new Color(0.12f, 1f, 1f)
				},
				{
					QualityCategory.Excellent,
					new Color(0f, 0.44f, 0.87f)
				},
				{
					QualityCategory.Masterwork,
					new Color(0.64f, 0.21f, 0.93f)
				},
				{
					QualityCategory.Legendary,
					new Color(1f, 0.5f, 0f)
				}
			}
		},
		{
			"Default",
			new Dictionary<QualityCategory, Color>
			{
				{
					QualityCategory.Awful,
					Color.red
				},
				{
					QualityCategory.Poor,
					new Color(0.62109375f, 0.40234375f, 0f)
				},
				{
					QualityCategory.Normal,
					Color.white
				},
				{
					QualityCategory.Good,
					Color.green
				},
				{
					QualityCategory.Excellent,
					Color.blue
				},
				{
					QualityCategory.Masterwork,
					new Color(89f / 128f, 33f / 64f, 95f / 128f)
				},
				{
					QualityCategory.Legendary,
					Color.yellow
				}
			}
		},
		{
			"Colorblind",
			new Dictionary<QualityCategory, Color>
			{
				{
					QualityCategory.Awful,
					new Color(0.86328125f, 51f / 64f, 0.46484375f)
				},
				{
					QualityCategory.Poor,
					new Color(17f / 32f, 17f / 128f, 0.33203125f)
				},
				{
					QualityCategory.Normal,
					Color.white
				},
				{
					QualityCategory.Good,
					new Color(17f / 32f, 51f / 64f, 119f / 128f)
				},
				{
					QualityCategory.Excellent,
					new Color(0.19921875f, 17f / 128f, 17f / 32f)
				},
				{
					QualityCategory.Masterwork,
					new Color(0.06640625f, 0.46484375f, 0.19921875f)
				},
				{
					QualityCategory.Legendary,
					new Color(0.86328125f, 51f / 64f, 0.46484375f)
				}
			}
		},
		{
			"FFXIV",
			new Dictionary<QualityCategory, Color>
			{
				{
					QualityCategory.Awful,
					new Color(0.41f, 0.41f, 0.41f)
				},
				{
					QualityCategory.Poor,
					new Color(0.62f, 0.62f, 0.62f)
				},
				{
					QualityCategory.Normal,
					Color.white
				},
				{
					QualityCategory.Good,
					new Color(1f, 0.5898438f, 0.8632813f)
				},
				{
					QualityCategory.Excellent,
					new Color(0.2416077f, 0.7929688f, 0.3406804f)
				},
				{
					QualityCategory.Masterwork,
					new Color(15f / 128f, 0.3999634f, 1f)
				},
				{
					QualityCategory.Legendary,
					new Color(0.5544863f, 0.2177124f, 123f / 128f)
				}
			}
		}
	};

	public Dictionary<QualityCategory, Color> Colors;

	public bool FullLabel;

	public ColorSettings()
	{
		Colors = Presets["Default"];
	}

	public override void ExposeData()
	{
		Scribe_Collections.Look(ref Colors, "colors", LookMode.Value, LookMode.Value);
		Scribe_Values.Look(ref FullLabel, "fullLabel", defaultValue: false);
		if (Colors == null)
		{
			Colors = Presets["Default"];
		}
	}
}
