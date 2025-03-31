using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;   // Honnan essen le a kocka?
    [SerializeField] private GameObject dicePrefab;  // Kocka prefab
    [SerializeField] private Transform groundPlane;  // Hova essen le?
    [SerializeField] private GameObject okBtn;
    private Vector3 resetCameraPosition;
    private List<GameObject> spawnedDice = new List<GameObject>();
    private void Start()
    {
        resetCameraPosition = Camera.main.transform.position;
    }
    public void RollDice(int diceNum)
    {
        Camera.main.transform.position = resetCameraPosition;
        groundPlane.gameObject.SetActive(true);
        StartCoroutine(RollDiceCoroutine(diceNum));
    }
    public void ReRollDice(int num, List<int> results)
    {
        Camera.main.transform.position = resetCameraPosition;
        StartCoroutine(RerollDiceCoroutine(num, results));
    }
    IEnumerator RollDiceCoroutine(int diceNum)
    {
        // T�r�lj�k a r�gi kock�kat
        foreach (var die in spawnedDice)
        {
            Destroy(die);
        }
        spawnedDice.Clear();
        // L�trehozzuk az �j kock�kat
        for (int i = 0; i < diceNum; i++)
        {
            GameObject dice = Instantiate(dicePrefab, spawnPoint.position + new Vector3(i * 1.5f, 0, 0), Random.rotation);
            Rigidbody rb = dice.GetComponent<Rigidbody>();

            // Egy v�letlenszer� er�t adunk neki, hogy dob�snak t�nj�n
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);

            spawnedDice.Add(dice);
        }

        // Megv�rjuk, hogy a kock�k leessenek �s meg�lljanak
        yield return new WaitForSeconds(2f);
    }
    public IEnumerator WaitForDiceToStop(System.Action<List<int>> onResultReady)
    {
        bool allStopped = false;

        while (!allStopped)
        {
            allStopped = true;

            foreach (var die in spawnedDice)
            {
                Rigidbody rb = die.GetComponent<Rigidbody>();

                if (!rb.IsSleeping()) // Ha m�g mozog, nem �llt meg
                {
                    allStopped = false;
                    break;
                }
            }

            yield return null; // V�runk egy frame-et, majd �jra ellen�rizz�k
        }

        // Amikor minden kocka meg�llt, akkor olvassuk le az eredm�nyeket
        List<int> results = new List<int>();
        foreach (var die in spawnedDice)
        {
            results.Add(ReadDieValue(die));
        }

        // Callback, hogy jelezz�k a dob�s eredm�ny�t
        onResultReady?.Invoke(results);
    }
    private int ReadDieValue(GameObject die)
    {
        Transform dieTransform = die.transform;

        // Az arcok lehets�ges ir�nyai �s azokhoz tartoz� �rt�kek
        Dictionary<Vector3, int> faceValues = new Dictionary<Vector3, int>()
        {
        { dieTransform.up, 2 },    // Felfel� n�z�
        { -dieTransform.up, 5 },   // Lefel� n�z�
        { dieTransform.right, 4 }, // Jobbra n�z�
        { -dieTransform.right, 3 },// Balra n�z�
        { dieTransform.forward, 1 },// El�re n�z�
        { -dieTransform.forward, 6 } // H�tra n�z�
        };

        Vector3 upVector = Vector3.up;
        float maxDot = -1;
        int bestValue = 1;

        foreach (var face in faceValues)
        {
            float dot = Vector3.Dot(face.Key, upVector); // Mennyire n�z felfel�?
            if (dot > maxDot)
            {
                maxDot = dot;
                bestValue = face.Value;
            }
        }

        return bestValue;
    }
    IEnumerator RerollDiceCoroutine(int rerollThreshold, List<int> results)
    {
        List<GameObject> reRolledDice = new List<GameObject>();
        for (int i = 0; i < spawnedDice.Count; i++)
        {
            if (results[i] < rerollThreshold)
                reRolledDice.Add(spawnedDice[i]);
        }
        for (int i = 0; i < reRolledDice.Count; i++)
        {
            var die = reRolledDice[i];
            spawnedDice.Remove(reRolledDice[i]);
            Destroy(die);

            GameObject newDie = Instantiate(dicePrefab, spawnPoint.position + new Vector3(i * 1.5f, 0, 0), Random.rotation);
            Rigidbody rb = newDie.GetComponent<Rigidbody>();

            // Egy v�letlenszer� er�t adunk neki, hogy dob�snak t�nj�n
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);

            spawnedDice.Add(newDie);
        }
        yield return new WaitForSeconds(2f);
    }
    public void RollFinished(string data, List<int> results)
    {
        okBtn.gameObject.SetActive(true);
        okBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        okBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            GameController.Instance.OnOkRollDiceClicked(data, results);
            OnOkButtonClicked();
        });
    }
    public void OnOkButtonClicked()
    {
        foreach (var die in spawnedDice)
        {
            Destroy(die);
        }
        spawnedDice.Clear();
        groundPlane.gameObject.SetActive(false);
        okBtn.gameObject.SetActive(false);
    }
}
