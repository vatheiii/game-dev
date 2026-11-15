using System.Collections.Generic;
using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform firstSection;

    private const float SectionLength = 100f;
    private const int SectionCount = 3;
    private readonly Queue<Transform> sections = new();

    private void Start()
    {
        if (player == null || firstSection == null)
        {
            Debug.LogError("Assign Player and First Section on TrackSpawner.", this);
            enabled = false;
            return;
        }

        sections.Enqueue(firstSection);

        for (int i = 1; i < SectionCount; i++)
        {
            Vector3 position = firstSection.position + Vector3.forward * (i * SectionLength);
            sections.Enqueue(Instantiate(firstSection, position, firstSection.rotation));
        }
    }

    private void Update()
    {
        while (player.position.z >= sections.Peek().position.z + SectionLength)
        {
            Transform oldest = sections.Dequeue();
            oldest.position += Vector3.forward * (SectionLength * SectionCount);
            foreach (Collider obstacle in oldest.GetComponentsInChildren<Collider>(true))
            {
                if (obstacle.CompareTag("Obstacle"))
                    obstacle.enabled = true;
            }
            sections.Enqueue(oldest);
        }
    }
}