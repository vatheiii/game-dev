internal static class TrackLayouts
{
    internal const int Left = -1;
    internal const int Middle = 0;
    internal const int Right = 1;

    internal enum Layout { Easy, MirroredEasy, Medium, Hard }
    internal enum Piece { Log, Boulder, MovingLog, Fish, Shield, Bomb }

    // Each placement gives the distance into the section, the lane, and the object to spawn.
    private static readonly (int Z, int Lane, Piece Piece)[] Easy =
    {
        (8, Middle, Piece.Log),
        (22, Right, Piece.Boulder), (22, Left, Piece.Fish),
        (36, Left, Piece.Log), (36, Right, Piece.Log),
        (50, Middle, Piece.Boulder), (50, Right, Piece.Shield),
        (64, Right, Piece.Boulder), (64, Middle, Piece.Log),
        (78, Left, Piece.Log), (78, Right, Piece.Log),
        (92, Middle, Piece.Log)
    };

    private static readonly (int Z, int Lane, Piece Piece)[] Medium =
    {
        (8, Left, Piece.Log), (8, Right, Piece.Fish),
        (20, Middle, Piece.Boulder), (20, Right, Piece.Log),
        (32, Right, Piece.Boulder), (32, Left, Piece.Shield),
        (44, Middle, Piece.MovingLog),
        (56, Left, Piece.Boulder), (56, Right, Piece.Bomb),
        (68, Middle, Piece.Log), (68, Right, Piece.Boulder),
        (80, Left, Piece.Boulder), (80, Right, Piece.Log),
        (92, Middle, Piece.Log)
    };

    private static readonly (int Z, int Lane, Piece Piece)[] Hard =
    {
        (5, Middle, Piece.Boulder),
        (15, Left, Piece.Boulder), (15, Middle, Piece.Log), (15, Right, Piece.Shield),
        (27, Left, Piece.Log), (27, Right, Piece.Boulder),
        (37, Left, Piece.Boulder), (37, Right, Piece.Bomb),
        (49, Middle, Piece.MovingLog),
        (59, Right, Piece.Boulder),
        (65, Right, Piece.Fish),
        (71, Left, Piece.Boulder), (71, Right, Piece.Log),
        (81, Left, Piece.Log), (81, Right, Piece.Boulder),
        (93, Left, Piece.Log), (93, Middle, Piece.Boulder)
    };

    internal static (int Z, int Lane, Piece Piece)[] GetPlacements(Layout layout) => layout switch
    {
        Layout.Medium => Medium,
        Layout.Hard => Hard,
        _ => Easy
    };
}
