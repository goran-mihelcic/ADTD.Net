namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Predefined Text Styles
    /// </summary>
    public enum TextStyle
    {
        Normal, 
        Title,
        Header1,
        Header2,
        Bullet,
        NormalItalic,
        NormalBold,
        SmallNormal,
        SmallNormalItalic,
        SmallNormalBold
    }

    public enum ShapeColors
    {
        Brown,
        DarkBlue,
        DarkRed,
        DarkGreen,
        DarkOrange,
        DarkViolet,
        Gold,
        Gray,
        Indigo,
        Ivory,
        Lavender,
        Magenta,
        Olive,
        Pink,
        Purple,
        Yellow,
        Silver,
        SpringGreen,
        Tomato,
        RosyBrown,
        Sienna,
        SteelBlue,
        SlateGray,
        Salmon,
        PaleGoldenrod,
        PaleGreen,
        PaleTurquoise,
        PaleVioletRed,
        Maroon,
        MediumAquamarine,
        MediumBlue,
        MediumOrchid,
        MediumPurple,
        MediumSeaGreen,
        MediumSlateBlue,
        MediumSpringGreen,
        MediumTurquoise,
        MediumVioletRed,
        MidnightBlue,
        MintCream,
        MistyRose,
        Moccasin,
        LightBlue,
        LightCoral,
        LightCyan,
        LightGoldenrodYellow,
        LightGray,
        LightGreen,
        LightPink,
        LightSalmon
    }


    /// <summary>
    /// Types for Custom properties - redundant (Microsoft.Sirona.Services.csproj)
    /// Created to avoid cyclic dependencies
    /// In the future this enum should be extracted from both projects
    /// </summary>
    public enum PropertyTypes : int
    {
        YesNo,
        Text,
        DateTime,
        NumberInteger,
        NumberDouble
    }

    /// <summary>
    /// Namespaces in Visio document
    /// </summary>
    public enum XVisioNamespace
    {
        Main,
        TextSettings,
        Relationships
    }

}
