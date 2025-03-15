using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;   // Honnan essen le a kocka?
    [SerializeField] private GameObject dicePrefab;  // Kocka prefab
    [SerializeField] private Transform groundPlane;  // Hova essen le?

    private List<GameObject> spawnedDice = new List<GameObject>();

    public void RollDice(List<int> predefinedResults)
    {
        groundPlane.gameObject.SetActive(true);
        StartCoroutine(RollDiceCoroutine(predefinedResults));
    }

    IEnumerator RollDiceCoroutine(List<int> predefinedResults)
    {
        List<Rigidbody> diceRigidbodies = new List<Rigidbody>();
        // Töröljük a régi kockákat
        foreach (var die in spawnedDice)
        {
            Destroy(die);
        }
        spawnedDice.Clear();

        // Létrehozzuk az új kockákat
        for (int i = 0; i < predefinedResults.Count; i++)
        {
            GameObject dice = Instantiate(dicePrefab, spawnPoint.position + new Vector3(i * 1.5f, 0, 0), Random.rotation);
            Rigidbody rb = dice.GetComponent<Rigidbody>();

            diceRigidbodies.Add(rb);

            // Egy véletlenszerû erõt adunk neki, hogy dobásnak tûnjön
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);

            spawnedDice.Add(dice);
        }

        // Várunk, amíg az összes kocka lelassul
        yield return new WaitUntil(() =>
            diceRigidbodies.All(rb => rb.velocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f));

        // Finom igazítás az elõre meghatározott eredményekre
        for (int i = 0; i < spawnedDice.Count; i++)
        {
            AlignDiceToResult(spawnedDice[i], predefinedResults[i]);
        }
    }
    void AlignDiceToResult(GameObject die, int result)
    {
        // A dobás végeredményéhez tartozó megfelelõ forgatások
        Quaternion[] diceRotations = new Quaternion[]
        {
        Quaternion.Euler(0, 0, 0),   // 1-es oldal felfelé
        Quaternion.Euler(0, 0, 180), // 2-es oldal felfelé
        Quaternion.Euler(0, 0, 90),  // 3-as oldal felfelé
        Quaternion.Euler(0, 0, -90), // 4-es oldal felfelé
        Quaternion.Euler(90, 0, 0),  // 5-ös oldal felfelé
        Quaternion.Euler(-90, 0, 0), // 6-os oldal felfelé
        };

        // A kocka végsõ igazítása a kívánt eredményre
        die.transform.rotation = diceRotations[result - 1];
    }
}
