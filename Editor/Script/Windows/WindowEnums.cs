using UnityEngine;


namespace binc.PixelAnimator.Editor.Windows{

    public enum PropertyFocusEnum{
        HitBox,
        Sprite
    }

    public enum WindowEnum{
        Property,
        Timeline,
        Canvas,
        none
    }

    //Box handles.
    public enum HandleType{
        TopLeft,
        TopCenter,
        TopRight,
        LeftCenter,
        BottomRight,
        BottomCenter,
        BottomLeft,
        RightCenter,
        Middle,
        None
    }


}
