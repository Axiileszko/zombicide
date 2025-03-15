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
        // T�r�lj�k a r�gi kock�kat
        foreach (var die in spawnedDice)
        {
            Destroy(die);
        }
        spawnedDice.Clear();

        // L�trehozzuk az �j kock�kat
        for (int i = 0; i < predefinedResults.Count; i++)
        {
            GameObject dice = Instantiate(dicePrefab, spawnPoint.position + new Vector3(i * 1.5f, 0, 0), Random.rotation);
            Rigidbody rb = dice.GetComponent<Rigidbody>();

            diceRigidbodies.Add(rb);

            // Egy v�letlenszer� er�t adunk neki, hogy dob�snak t�nj�n
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);

            spawnedDice.Add(dice);
        }

        // V�runk, am�g az �sszes kocka lelassul
        yield return new WaitUntil(() =>
            diceRigidbodies.All(rb => rb.velocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f));

        // Finom igaz�t�s az el�re meghat�rozott eredm�nyekre
        for (int i = 0; i < spawnedDice.Count; i++)
        {
            AlignDiceToResult(spawnedDice[i], predefinedResults[i]);
        }
    }
    void AlignDiceToResult(GameObject die, int result)
    {
        // A dob�s v�geredm�ny�hez tartoz� megfelel� forgat�sok
        Quaternion[] diceRotations = new Quaternion[]
        {
        Quaternion.Euler(0, 0, 0),   // 1-es oldal felfel�
        Quaternion.Euler(0, 0, 180), // 2-es oldal felfel�
        Quaternion.Euler(0, 0, 90),  // 3-as oldal felfel�
        Quaternion.Euler(0, 0, -90), // 4-es oldal felfel�
        Quaternion.Euler(90, 0, 0),  // 5-�s oldal felfel�
        Quaternion.Euler(-90, 0, 0), // 6-os oldal felfel�
        };

        // A kocka v�gs� igaz�t�sa a k�v�nt eredm�nyre
        die.transform.rotation = diceRotations[result - 1];
    }
}
