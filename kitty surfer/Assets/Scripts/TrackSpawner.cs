using UnityEngine;
using static TrackLayouts;

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

    private const int LaneCount = Right - Left + 1;

    private static int previousRunEasyLaneOffset = -1;

    // Unity restores these fields when scripts reload during Play Mode.
    private PlayerController playerController;
    private Transform[] sections;
    private Transform[] activeLayouts;
    private int oldestSectionIndex;
    private int nextSectionDistance;
    private int easyLaneOffset;
    private Layout previousLayout;
    private int consecutiveLayoutCount;

    private void Start()
    {
        if (player == null || !player.TryGetComponent(out playerController) || firstSection == null)
        {
            Debug.LogError("Assign a Player with PlayerController and a First Section on TrackSpawner.", this);
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
        easyLaneOffset = previousRunEasyLaneOffset < 0
            ? Random.Range(0, LaneCount)
            : (previousRunEasyLaneOffset + Random.Range(1, LaneCount)) % LaneCount;
        previousRunEasyLaneOffset = easyLaneOffset;

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
        Layout layout = ChooseLayout(nextSectionDistance);
        consecutiveLayoutCount = layout == previousLayout ? consecutiveLayoutCount + 1 : 1;
        previousLayout = layout;
        nextSectionDistance += SectionLength;

        bool isEasy = layout == Layout.Easy || layout == Layout.MirroredEasy;

        Transform section = sections[sectionIndex];
        GameObject content = new GameObject(layout.ToString());
        // Keep movers inactive until their starting positions have been assigned.
        content.SetActive(false);
        content.transform.SetParent(section, false);
        foreach (var placement in TrackLayouts.GetPlacements(layout))
        {
            int lane = layout == Layout.MirroredEasy ? -placement.Lane : placement.Lane;
            if (isEasy)
                lane = (lane - Left + easyLaneOffset) % LaneCount + Left;

            Transform template = placement.Piece switch
            {
                Piece.Log => lane == Right ? rightLog : leftLog,
                Piece.Boulder => boulder,
                Piece.MovingLog => movingLog,
                Piece.Fish => fish,
                Piece.Shield => shield,
                _ => bomb
            };
            Vector3 lanePosition = section.position;
            lanePosition.x = playerController.GetLaneX(lane);
            float localLaneX = section.InverseTransformPoint(lanePosition).x;
            Transform obstacle = Instantiate(template, content.transform, false);
            obstacle.localPosition = new Vector3(localLaneX, template.localPosition.y, placement.Z);
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
