using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace RimWorld;

public class CustomDialog_GlowerPicker : Dialog_GlowerColorPicker
{
    public CustomDialog_GlowerPicker(CompGlower glower, IList<CompGlower> extraGlowers, Widgets.ColorComponents visible, Widgets.ColorComponents editable)
        : base(glower, extraGlowers, visible, editable)
    {
    }

    protected override bool ShowDarklight => false;
    public override Vector2 InitialSize => new Vector2(700f, 550f);
}