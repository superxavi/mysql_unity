using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SnakeController : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 direction = Vector3.forward;
    private int score = 0;
    public GameObject foodPrefab;

    void Start()
    {
        SpawnFood();
    }

    void Update()
    {
        // Movimiento
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector3.forward;
        if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector3.back;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector3.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector3.right;

        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            score += 10;
            Destroy(other.gameObject);
            SpawnFood();

            if (score >= 50) // Cuando alcanza 50 puntos
            {
                StartCoroutine(SaveScore());
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            }
        }
    }

    void SpawnFood()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-9f, 9f),
            0.5f,
            Random.Range(-9f, 9f)
        );
        Instantiate(foodPrefab, randomPos, Quaternion.identity);
    }

    IEnumerator SaveScore()
    {
        WWWForm form = new WWWForm();
        string playerName = "Jugador1"; // Puedes hacerlo dinámico con un UI Input

        string json = JsonUtility.ToJson(new ScoreData(playerName, score));
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest webRequest = new UnityWebRequest("http://localhost/save_score.php", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Respuesta: " + webRequest.downloadHandler.text);
            }
        }
    }
}

[System.Serializable]
public class ScoreData
{
    public string player_name;
    public int score;

    public ScoreData(string name, int score)
    {
        this.player_name = name;
        this.score = score;
    }
}