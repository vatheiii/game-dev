using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform firstSection;

    private const float SectionLength = 100f;
    private const int SectionCount = 3;

    // Unity restores these fields when scripts reload during Play Mode.
    private Transform[] sections;
    private int oldestSectionIndex;

    private void Start()
    {
        if (player == null || firstSection == null)
        {
            Debug.LogError("Assign Player and First Section on TrackSpawner.", this);
            enabled = false;
            return;
        }

        sections = new Transform[SectionCount];
        sections[0] = firstSection;
        oldestSectionIndex = 0;

        for (int i = 1; i < SectionCount; i++)
        {
            Vector3 position = firstSection.position + Vector3.forward * (i * SectionLength);
            sections[i] = Instantiate(firstSection, position, firstSection.rotation);
        }
    }

    private void Update()
    {
        if (sections == null || sections.Length == 0)
            return;

        while (player.position.z >= sections[oldestSectionIndex].position.z + SectionLength)
        {
            Transform oldest = sections[oldestSectionIndex];
            oldest.position += Vector3.forward * (SectionLength * SectionCount);
            oldestSectionIndex = (oldestSectionIndex + 1) % sections.Length;

            foreach (ObstacleMover mover in oldest.GetComponentsInChildren<ObstacleMover>())
            {
                mover.ResetForRecycle();
            }

            foreach (Collider obstacle in oldest.GetComponentsInChildren<Collider>(true))
            {
                if (obstacle.CompareTag("Obstacle"))
                {
                    obstacle.enabled = true;
                }
            }

            foreach (Transform child in oldest.GetComponentsInChildren<Transform>(true))
            {
                if (child.CompareTag("Fish") || child.CompareTag("Shield") || child.CompareTag("Bomb"))
                    child.gameObject.SetActive(true);
            }
        }
    }
}
