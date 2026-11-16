using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform firstSection;
    [SerializeField] private Transform leftLog;
    [SerializeField] private Transform rightLog;
    [SerializeField] private Transform boulder;
    [SerializeField] private Transform movingLog;
    [SerializeField] private Transform fish;
    [SerializeField] private Transform shield;
    [SerializeField] private Transform bomb;

    private const int SectionLength = 100;
    private const int SectionCount = 3;
    private const int MediumDistance = 250;
    private const int HardDistance = 600;

    private const int Left = -1;
    private const int Middle = 0;
    private const int Right = 1;

    private enum Layout { Easy, MirroredEasy, Medium, Hard }
    private enum Piece { Log, Boulder, MovingLog, Fish, Shield, Bomb }

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

    // Unity restores these fields when scripts reload during Play Mode.
    private Transform[] sections;
    private Transform[] activeLayouts;
    private int oldestSectionIndex;
    private int nextSectionDistance;
    private Layout previousLayout;
    private int consecutiveLayoutCount;

    private void Start()
    {
        if (player == null || firstSection == null)
        {
            Debug.LogError("Assign Player and First Section on TrackSpawner.", this);
            enabled = false;
            return;
        }

        Transform[] templates = { leftLog, rightLog, boulder, movingLog, fish, shield, bomb };
        foreach (Transform template in templates)
        {
            if (template == null || template.parent != firstSection)
            {
                Debug.LogError("Assign all track obstacle templates from the first section.", this);
                enabled = false;
                return;
            }
        }

        // Move the scene's original obstacles out of the ground before cloning it.
        GameObject templateRoot = new GameObject("Obstacle Templates");
        templateRoot.SetActive(false);
        templateRoot.transform.SetParent(transform, false);
        foreach (Transform template in templates)
            template.SetParent(templateRoot.transform, false);

        sections = new Transform[SectionCount];
        activeLayouts = new Transform[SectionCount];
        sections[0] = firstSection;
        oldestSectionIndex = 0;
        nextSectionDistance = 0;
        consecutiveLayoutCount = 0;

        for (int i = 1; i < SectionCount; i++)
        {
            Vector3 position = firstSection.position + Vector3.forward * (i * SectionLength);
            sections[i] = Instantiate(firstSection, position, firstSection.rotation);
        }

        for (int i = 0; i < SectionCount; i++)
            ApplyNextLayout(i);
    }

    private void Update()
    {
        if (sections == null || sections.Length == 0 || activeLayouts == null || activeLayouts.Length != sections.Length)
            return;

        while (player.position.z >= sections[oldestSectionIndex].position.z + SectionLength)
        {
            activeLayouts[oldestSectionIndex].gameObject.SetActive(false);
            Destroy(activeLayouts[oldestSectionIndex].gameObject);
            sections[oldestSectionIndex].position += Vector3.forward * (SectionLength * SectionCount);
            ApplyNextLayout(oldestSectionIndex);
            oldestSectionIndex = (oldestSectionIndex + 1) % sections.Length;
        }
    }

    private void ApplyNextLayout(int sectionIndex)
    {
        Layout layout = nextSectionDistance == 0 ? Layout.Easy : ChooseLayout(nextSectionDistance);
        consecutiveLayoutCount = layout == previousLayout ? consecutiveLayoutCount + 1 : 1;
        previousLayout = layout;
        nextSectionDistance += SectionLength;

        var placements = layout switch
        {
            Layout.Medium => Medium,
            Layout.Hard => Hard,
            _ => Easy
        };

        GameObject content = new GameObject(layout.ToString());
        // Keep movers inactive until their starting positions have been assigned.
        content.SetActive(false);
        content.transform.SetParent(sections[sectionIndex], false);
        foreach (var placement in placements)
        {
            int lane = layout == Layout.MirroredEasy ? -placement.Lane : placement.Lane;
            Transform template = placement.Piece switch
            {
                Piece.Log => lane == Right ? rightLog : leftLog,
                Piece.Boulder => boulder,
                Piece.MovingLog => movingLog,
                Piece.Fish => fish,
                Piece.Shield => shield,
                _ => bomb
            };
            float laneX = lane switch
            {
                Left => leftLog.localPosition.x,
                Right => rightLog.localPosition.x,
                _ => boulder.localPosition.x
            };
            Transform obstacle = Instantiate(template, content.transform, false);
            obstacle.localPosition = new Vector3(laneX, template.localPosition.y, placement.Z);
            obstacle.gameObject.SetActive(true);
        }

        activeLayouts[sectionIndex] = content.transform;
        content.SetActive(true);
    }

    private Layout ChooseLayout(int distance)
    {
        int available = distance >= HardDistance ? 4 : distance >= MediumDistance ? 3 : 2;
        int extraHardestRolls = available - 2;
        int choice = Mathf.Min(Random.Range(0, available + extraHardestRolls), available - 1);
        if ((Layout)choice == previousLayout && consecutiveLayoutCount >= 2)
            choice = (choice + Random.Range(1, available)) % available;

        return (Layout)choice;
    }
}
