using System;
using RimWorld;
using Verse;

namespace QualityColors;

public class FakeGlower : CompGlower
{
	private readonly ColorInt curColor;

	private readonly Action<ColorInt> onColor;

	public override ColorInt GlowColor
	{
		get
		{
			return curColor;
		}
		set
		{
			onColor(value);
		}
	}

	protected override bool ShouldBeLitNow => false;

	public FakeGlower(ColorInt curColor, Action<ColorInt> onColor)
	{
		parent = new ThingWithComps();
		this.curColor = curColor;
		this.onColor = onColor;
		props = new CompProperties_Glower
		{
			glowColor = curColor
		};
	}
}
